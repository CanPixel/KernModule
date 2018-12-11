using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

using System.Numerics;
using DSPLib;

namespace AlgorithmicBeatDetection {
    public class AnalyzeManager : MonoBehaviour {

        int numChannels;
        int numTotalSamples;
        int sampleRate;
        float clipLength;
        float[] multiChannelSamples;
        SpectralFluxAnalyzer SpectralFluxAnalyzer;

        private bool spectrumThreaded;

        public float Duration {
            get {
                return audioClip.length;
            }
            set { }
        }
        public Dictionary<float, float> VolumePeaks {
            get {
                Dictionary<float, float> tmp = null;
                foreach (SpectralFluxInfo info in SpectralFluxAnalyzer.spectralFluxSamplesAll) {
                    if (info.isPeak)
                        tmp.Add(info.time, info.spectralFlux);
                }
                return tmp;
            }
            set { }
        } //Holds time and volume.
        public Dictionary<float, float> LowFreqPeaks {
            get {
                Dictionary<float, float> tmp = null;
                foreach (SpectralFluxInfo info in SpectralFluxAnalyzer.spectralFluxSamplesBass) {
                    if (info.isPeak)
                        tmp.Add(info.time, info.spectralFlux);
                }
                return tmp;
            }
            set { }
        } //Holds time and frequency.
        public Dictionary<float, float> MidFreqPeaks {
            get {
                Dictionary<float, float> tmp = null;
                foreach (SpectralFluxInfo info in SpectralFluxAnalyzer.spectralFluxSamplesMids) {
                    if (info.isPeak)
                        tmp.Add(info.time, info.spectralFlux);
                }
                return tmp;
            }
            set { }
        } //Holds time and frequency.
        public Dictionary<float, float> HighFreqPeaks {
            get {
                Dictionary<float, float> tmp = null;
                foreach (SpectralFluxInfo info in SpectralFluxAnalyzer.spectralFluxSamplesHighs) {
                    if (info.isPeak)
                        tmp.Add(info.time, info.spectralFlux);
                }
                return tmp;
            }
            set { }
        } //Holds time and frequency.

        private void StartAnalyze(AudioClip aud) {
            SpectralFluxAnalyzer = new SpectralFluxAnalyzer();

            // Need all audio samples.  If in stereo, samples will return with left and right channels interweaved
            // [L,R,L,R,L,R]
            multiChannelSamples = new float[aud.samples * aud.channels];
            numChannels = aud.channels;
            numTotalSamples = aud.samples;
            clipLength = aud.length;

            // We are not evaluating the audio as it is being played by Unity, so we need the clip's sampling rate
            this.sampleRate = aud.frequency;

            aud.GetData(multiChannelSamples, 0);
            Debug.Log("GetData done");

            Thread bgThread = new Thread(this.getFullSpectrumThreaded);
            bgThread.Start();
            Debug.Log("Starting Background Thread");
        }

        public SongInfo GetSongInfo(AudioClip aud) {
            SongInfo songInfo = new SongInfo();

            StartAnalyze(aud);

            songInfo.duration = aud.length;
            while (!spectrumThreaded) {
                if (spectrumThreaded) {
                    break;
                }
            }
            songInfo.volumePeaks = VolumePeaks;
            songInfo.lowFreqPeaks = LowFreqPeaks;
            songInfo.midFreqPeaks = MidFreqPeaks;
            songInfo.highFreqPeaks = HighFreqPeaks;

            return songInfo;
        }

        public float getTimeFromIndex(int index) {
            return ((1f / (float)this.sampleRate) * index);
        }

        public void getFullSpectrumThreaded() {
            try {
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

                Debug.Log("Combine Channels done");
                Debug.Log(preProcessedSamples.Length);

                // Once we have our audio sample data prepared, we can execute an FFT to return the spectrum data over the time domain
                int spectrumSampleSize = 1024;
                int iterations = preProcessedSamples.Length / spectrumSampleSize;

                FFT fft = new FFT();
                fft.Initialize((UInt32)spectrumSampleSize);

                Debug.Log(string.Format("Processing {0} time domain samples for FFT", iterations));
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
                    float curSongTime = getTimeFromIndex(i) * spectrumSampleSize;

                    // Send our magnitude data off to our Spectral Flux Analyzer to be analyzed for peaks
                    SpectralFluxAnalyzer.analyzeSpectrum(Array.ConvertAll(scaledFFTSpectrum, x => (float)x), curSongTime, 0); //Analyze Bass
                    SpectralFluxAnalyzer.analyzeSpectrum(Array.ConvertAll(scaledFFTSpectrum, x => (float)x), curSongTime, 1); //Analyze Mids
                    SpectralFluxAnalyzer.analyzeSpectrum(Array.ConvertAll(scaledFFTSpectrum, x => (float)x), curSongTime, 2); //Analyze Highs
                    SpectralFluxAnalyzer.analyzeSpectrum(Array.ConvertAll(scaledFFTSpectrum, x => (float)x), curSongTime, 3); //Analyze All
                }

                Debug.Log("Spectrum Analysis done");
                Debug.Log("Background Thread Completed");
                spectrumThreaded = true;

            }
            catch (Exception e) {
                // Catch exceptions here since the background thread won't always surface the exception to the main thread
                Debug.Log(e.ToString());
            }
        }
    }

    public class SongInfo {
        public float duration;
        public Dictionary<float, float> volumePeaks;
        public Dictionary<float, float> lowFreqPeaks;
        public Dictionary<float, float> midFreqPeaks;
        public Dictionary<float, float> highFreqPeaks;
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

        public void setCurSpectrum(float[] spectrum) {
            curSpectrum.CopyTo(prevSpectrum, 0);
            spectrum.CopyTo(curSpectrum, 0);
        }

        public void analyzeSpectrum(float[] spectrum, float time, int freqRange) {
            // Set spectrum
            setCurSpectrum(spectrum);

            spectralFluxSamples.Clear();
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

                if (curPeak) {
                    spectralFluxSamples[indexToDetectPeak].isPeak = true;
                }
                indexToProcess++;
            }
            //Copy samples to appropiate list of samples.
            if (freqRange == 0) {
                spectralFluxSamplesBass = spectralFluxSamples;
            }
            else if (freqRange == 1) {
                spectralFluxSamplesMids = spectralFluxSamples;
            }
            else if (freqRange == 2) {
                spectralFluxSamplesHighs = spectralFluxSamples;
            }
            else if (freqRange == 3) {
                spectralFluxSamplesAll = spectralFluxSamples;
            }

            else {
                Debug.Log(string.Format("Not ready yet.  At spectral flux sample size of {0} growing to {1}", spectralFluxSamples.Count, thresholdWindowSize));
            }
        }

        float calculateRectifiedSpectralFlux(float freqRangeStart, float freqRangeEnd) {
            float hertzPerBin = (float)AudioSettings.outputSampleRate / 2f / numSamples;
            int targetStartIndex = Mathf.RoundToInt(freqRangeStart / hertzPerBin);
            int targetEndIndex = Mathf.RoundToInt(freqRangeEnd / hertzPerBin);

            float sum = 0f;

            // Aggregate positive changes in spectrum data
            for (int i = targetStartIndex; i < targetEndIndex; i++) {
                sum += Mathf.Max(0f, curSpectrum[i] - prevSpectrum[i]);
            }
            return sum;
        }

        float getFluxThreshold(int spectralFluxIndex) {
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

        float getPrunedSpectralFlux(int spectralFluxIndex) {
            return Mathf.Max(0f, spectralFluxSamples[spectralFluxIndex].spectralFlux - spectralFluxSamples[spectralFluxIndex].threshold);
        }

        bool isPeak(int spectralFluxIndex) {
            if (spectralFluxSamples[spectralFluxIndex].prunedSpectralFlux > spectralFluxSamples[spectralFluxIndex + 1].prunedSpectralFlux &&
                spectralFluxSamples[spectralFluxIndex].prunedSpectralFlux > spectralFluxSamples[spectralFluxIndex - 1].prunedSpectralFlux) {
                return true;
            }
            else {
                return false;
            }
        }

        void logSample(int indexToLog) {
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