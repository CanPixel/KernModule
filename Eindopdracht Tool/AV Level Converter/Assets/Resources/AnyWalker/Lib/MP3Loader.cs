using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Numerics;
using DSPLib;

namespace AlgorithmicBeatDetection {
    public class AnalyzeManager {
        int numChannels;
        int numTotalSamples;
        int sampleRate;
        float[] multiChannelSamples;
        SpectralFluxAnalyzer SpectralFluxAnalyzer;

        private bool spectrumThreaded;
        private SongInfo songInfo;
        
        private Callback doneLoading, doneAnalyzing;
        public delegate void Callback(SongInfo s);

        private void StartAnalyze(AudioClip aud, SongInfo songInfo, Callback func) {
            SpectralFluxAnalyzer = new SpectralFluxAnalyzer();

            AnyWalker.ClearConsole();

            // Need all audio samples.  If in stereo, samples will return with left and right channels interweaved
            // [L,R,L,R,L,R]
            numChannels = aud.channels;
            numTotalSamples = Mathf.Clamp(aud.samples, 1, 500000); //Cap for too big audio files, otherwise loading takes too long
            multiChannelSamples = new float[numTotalSamples * numChannels];
            Debug.LogWarning("Starting Spectrum Analysis with " + numTotalSamples + " samples.");

            // We are not evaluating the audio as it is being played by Unity, so we need the clip's sampling rate
            this.sampleRate = aud.frequency;

            aud.GetData(multiChannelSamples, 0);
            doneAnalyzing = func;

            GetFullSpectrum();
        }

        private void FinishAnalyze(SongInfo songInfo) {
            songInfo.SetData(SpectralFluxAnalyzer.GetData());
            doneLoading(songInfo);
        }

        public void GetSongInfo(AudioClip aud, Callback done) {
            songInfo = new SongInfo();
            doneLoading = done;
            songInfo.duration = aud.length;
            StartAnalyze(aud, songInfo, FinishAnalyze);
        }

        public float GetTimeFromIndex(int index) {
            return ((1f / (float)this.sampleRate) * index);
        }

        private void GetFullSpectrum() {
                // We only need to retain the samples for combined channels over the time domain
                float[] preProcessedSamples = new float[this.numTotalSamples];

                int numProcessed = 0;
                float combinedChannelAverage = 0f;
                for (int i = 0; i < multiChannelSamples.Length; i++) {
                    combinedChannelAverage += multiChannelSamples[i];

                    // Each time we have processed all channels samples for a point in time, we will store the average of the channels combined
                    if ((i + 1) % this.numChannels == 0) {
                        preProcessedSamples[numProcessed] = combinedChannelAverage / this.numChannels;
                        numProcessed++;
                        combinedChannelAverage = 0f;
                    }
                }

                // Once we have our audio sample data prepared, we can execute an FFT to return the spectrum data over the time domain
                int spectrumSampleSize = 1024;
                int iterations = preProcessedSamples.Length / spectrumSampleSize;

                FFT fft = new FFT();
                fft.Initialize((UInt32)spectrumSampleSize);

                double[] sampleChunk = new double[spectrumSampleSize];
                for (int i = 0; i < iterations; i++) {
                    // Grab the current 1024 chunk of audio sample data
                    Array.Copy(preProcessedSamples, i * spectrumSampleSize, sampleChunk, 0, spectrumSampleSize);

                    // Apply our chosen FFT Window
                    double[] windowCoefs = DSP.Window.Coefficients(DSP.Window.Type.Hanning, (uint)spectrumSampleSize);
                    double[] scaledSpectrumChunk = DSP.Math.Multiply(sampleChunk, windowCoefs);
                    double scaleFactor = DSP.Window.ScaleFactor.Signal(windowCoefs);

                    // Perform the FFT and convert output (complex numbers) to Magnitude
                    Complex[] fftSpectrum = fft.Execute(scaledSpectrumChunk);
                    double[] scaledFFTSpectrum = DSPLib.DSP.ConvertComplex.ToMagnitude(fftSpectrum);
                    scaledFFTSpectrum = DSP.Math.Multiply(scaledFFTSpectrum, scaleFactor);

                    // These 1024 magnitude values correspond (roughly) to a single point in the audio timeline
                    float curSongTime = GetTimeFromIndex(i) * spectrumSampleSize;

                    // Send our magnitude data off to our Spectral Flux Analyzer to be analyzed for peaks
                    SpectralFluxAnalyzer.AnalyzeSpectrum(Array.ConvertAll(scaledFFTSpectrum, x => (float)x), curSongTime, 0); //Analyze Bass
                    SpectralFluxAnalyzer.AnalyzeSpectrum(Array.ConvertAll(scaledFFTSpectrum, x => (float)x), curSongTime, 1); //Analyze Mids
                    SpectralFluxAnalyzer.AnalyzeSpectrum(Array.ConvertAll(scaledFFTSpectrum, x => (float)x), curSongTime, 2); //Analyze Highs
                    SpectralFluxAnalyzer.AnalyzeSpectrum(Array.ConvertAll(scaledFFTSpectrum, x => (float)x), curSongTime, 3); //Analyze All
                }

                Debug.LogWarning("Spectrum Analysis done");
                if(doneAnalyzing != null) doneAnalyzing(songInfo);
                spectrumThreaded = true;
        }
    }

    public class SongInfo {
        public float duration {get;set;}

        private List<SpectralFluxInfo> volumePeaks, lowFreqPeaks, midFreqPeaks, highFreqPeaks;
        private  List<List<SpectralFluxInfo>> data;

        public void SetData(List<SpectralFluxInfo>[] list) {
            List<SpectralFluxInfo> lows = list[0], mids = list[1], highs = list[2], alls = list[3];
            lowFreqPeaks = new List<SpectralFluxInfo>();
            midFreqPeaks = new List<SpectralFluxInfo>();
            highFreqPeaks = new List<SpectralFluxInfo>();
            volumePeaks = new List<SpectralFluxInfo>();

            List<List<SpectralFluxInfo>> unfilteredList = new List<List<SpectralFluxInfo>>();
            data = new List<List<SpectralFluxInfo>>();
            data.Add(lowFreqPeaks);
            data.Add(midFreqPeaks);
            data.Add(highFreqPeaks);
            data.Add(volumePeaks);
            unfilteredList.Add(lows);
            unfilteredList.Add(mids);
            unfilteredList.Add(highs);
            unfilteredList.Add(alls);
            for(int i = 0; i < unfilteredList.Count; i++)
                foreach (SpectralFluxInfo info in list[i]) {
                    if (info.isPeak) data[i].Add(info);
            }
        }

        public int GetDataLength() {
            return data.Count;
        }

        public List<SpectralFluxInfo> GetData(int i) {
            return data[i];
        }
    }

    public class SpectralFluxInfo {
        public float time;
        public float spectralFlux;
        public float threshold;
        public float prunedSpectralFlux;
        public bool isPeak;
    }

    public class SpectralFluxAnalyzer {
        int numSamples = 1024;

        // Sensitivity multiplier to scale the average threshold.
        // In this case, if a rectified spectral flux sample is > 1.5 times the average, it is a peak
        float thresholdMultiplier = 1.5f;

        // Number of samples to average in our window
        int thresholdWindowSize = 50;

        public List<SpectralFluxInfo> spectralFluxSamples;

        public List<SpectralFluxInfo> spectralFluxSamplesBass;
        public List<SpectralFluxInfo> spectralFluxSamplesMids;
        public List<SpectralFluxInfo> spectralFluxSamplesHighs;
        public List<SpectralFluxInfo> spectralFluxSamplesAll;

        float[] curSpectrum;
        float[] prevSpectrum;

        int indexToProcess;

        public List<SpectralFluxInfo>[] GetData() {
            List<SpectralFluxInfo>[] list = new List<SpectralFluxInfo>[]{spectralFluxSamplesBass, spectralFluxSamplesMids, spectralFluxSamplesHighs, spectralFluxSamplesAll};
            return list;
        }

        public SpectralFluxAnalyzer() {
            spectralFluxSamples = new List<SpectralFluxInfo>();
            spectralFluxSamplesBass = new List<SpectralFluxInfo>();
            spectralFluxSamplesMids = new List<SpectralFluxInfo>();
            spectralFluxSamplesHighs = new List<SpectralFluxInfo>();
            spectralFluxSamplesAll = new List<SpectralFluxInfo>();

            // Start processing from middle of first window and increment by 1 from there
            indexToProcess = thresholdWindowSize / 2;

            curSpectrum = new float[numSamples];
            prevSpectrum = new float[numSamples];
        }

        public void SetCurSpectrum(float[] spectrum) {
            curSpectrum.CopyTo(prevSpectrum, 0);
            spectrum.CopyTo(curSpectrum, 0);
        }

        public void AnalyzeSpectrum(float[] spectrum, float time, int freqRange) {
            // Set spectrum
            SetCurSpectrum(spectrum);

           // spectralFluxSamples.Clear();
            float freqRangeStart;
            float freqRangeEnd;

            //Decide on the frequency range which we will analyze.
            if (freqRange == 0) {
                freqRangeStart = 20f;
                freqRangeEnd = 200f;
            }
            else if (freqRange == 1) {
                freqRangeStart = 200f;
                freqRangeEnd = 5000f;
            }
            else if (freqRange == 2) {
                freqRangeStart = 5000f;
                freqRangeEnd = 20000f;
            }
            else if (freqRange == 3) {
                freqRangeStart = 0f;
                freqRangeEnd = numSamples;
            }
            else {
                freqRangeStart = 0f;
                freqRangeEnd = numSamples;
                Debug.Log("No frequency range given");
            }

            // Get current spectral flux from spectrum
            SpectralFluxInfo curInfo = new SpectralFluxInfo();
            curInfo.time = time;
            curInfo.spectralFlux = calculateRectifiedSpectralFlux(freqRangeStart, freqRangeEnd);
            spectralFluxSamples.Add(curInfo);

            // We have enough samples to detect a peak
            if (spectralFluxSamples.Count >= thresholdWindowSize) {
                // Get Flux threshold of time window surrounding index to process
                spectralFluxSamples[indexToProcess].threshold = getFluxThreshold(indexToProcess);

                // Only keep amp amount above threshold to allow peak filtering
                spectralFluxSamples[indexToProcess].prunedSpectralFlux = getPrunedSpectralFlux(indexToProcess);

                // Now that we are processed at n, n-1 has neighbors (n-2, n) to determine peak
                int indexToDetectPeak = indexToProcess - 1;

                bool curPeak = isPeak(indexToDetectPeak);
                if (curPeak) spectralFluxSamples[indexToDetectPeak].isPeak = true;
                
                indexToProcess++;
            }
            //Copy samples to appropiate list of samples.
            if (freqRange == 0)  spectralFluxSamplesBass = spectralFluxSamples;
            else if (freqRange == 1) spectralFluxSamplesMids = spectralFluxSamples;
            else if (freqRange == 2) spectralFluxSamplesHighs = spectralFluxSamples;
            else if (freqRange == 3) spectralFluxSamplesAll = spectralFluxSamples;

            else Debug.Log(string.Format("Not ready yet.  At spectral flux sample size of {0} growing to {1}", spectralFluxSamples.Count, thresholdWindowSize));
        }

        protected float calculateRectifiedSpectralFlux(float freqRangeStart, float freqRangeEnd) {
            float hertzPerBin = (float)AudioSettings.outputSampleRate / 2f / numSamples;
            int targetStartIndex = Mathf.RoundToInt(freqRangeStart / hertzPerBin);
            int targetEndIndex = Mathf.RoundToInt(freqRangeEnd / hertzPerBin);

            float sum = 0f;

            // Aggregate positive changes in spectrum data
            for (int i = targetStartIndex; i < targetEndIndex; i++) sum += Mathf.Max(0f, curSpectrum[i] - prevSpectrum[i]);
            return sum;
        }

        protected float getFluxThreshold(int spectralFluxIndex) {
            // How many samples in the past and future we include in our average
            int windowStartIndex = Mathf.Max(0, spectralFluxIndex - thresholdWindowSize / 2);
            int windowEndIndex = Mathf.Min(spectralFluxSamples.Count - 1, spectralFluxIndex + thresholdWindowSize / 2);

            // Add up our spectral flux over the window
            float sum = 0f;
            for (int i = windowStartIndex; i < windowEndIndex; i++) {
                sum += spectralFluxSamples[i].spectralFlux;
            }

            // Return the average multiplied by our sensitivity multiplier
            float avg = sum / (windowEndIndex - windowStartIndex);
            return avg * thresholdMultiplier;
        }

        protected float getPrunedSpectralFlux(int spectralFluxIndex) {
            return Mathf.Max(0f, spectralFluxSamples[spectralFluxIndex].spectralFlux - spectralFluxSamples[spectralFluxIndex].threshold);
        }

        protected bool isPeak(int spectralFluxIndex) {
            if (spectralFluxSamples[spectralFluxIndex].prunedSpectralFlux > spectralFluxSamples[spectralFluxIndex + 1].prunedSpectralFlux &&
                spectralFluxSamples[spectralFluxIndex].prunedSpectralFlux > spectralFluxSamples[spectralFluxIndex - 1].prunedSpectralFlux) {
                return true;
            }
            else return false;
        }

        protected void logSample(int indexToLog) {
            int windowStart = Mathf.Max(0, indexToLog - thresholdWindowSize / 2);
            int windowEnd = Mathf.Min(spectralFluxSamples.Count - 1, indexToLog + thresholdWindowSize / 2);
            Debug.Log(string.Format(
                "Peak detected at song time {0} with pruned flux of {1} ({2} over thresh of {3}).\n" +
                "Thresh calculated on time window of {4}-{5} ({6} seconds) containing {7} samples.",
                spectralFluxSamples[indexToLog].time,
                spectralFluxSamples[indexToLog].prunedSpectralFlux,
                spectralFluxSamples[indexToLog].spectralFlux,
                spectralFluxSamples[indexToLog].threshold,
                spectralFluxSamples[windowStart].time,
                spectralFluxSamples[windowEnd].time,
                spectralFluxSamples[windowEnd].time - spectralFluxSamples[windowStart].time,
                windowEnd - windowStart
            ));
        }
    }
}