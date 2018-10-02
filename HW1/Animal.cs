using System;

public class Animal {
	//NOGSTEEDS REVOLUTIE VOOR DE HAAKJES PLZ!!
	public class Dog : Animal {
		protected override void Eat() {
			Console.WriteLine("Hondjeee lekker gegetuh");
		}
	}
	
	public class Cat : Animal {
		protected override void Eat() {
			Console.WriteLine("Katrina lekker gegetuh");
		}
	}
	
	public static void Main(string[] args) {
		Animal[] animals = new Animal[20];
		for(int i = 0; i < animals.Length / 2; i++) animals[i] = new Cat(); 
		for(int i = animals.Length / 2; i < animals.Length; i++) animals[i] = new Dog();
		foreach(Animal i in animals) i.Eat();
	}
	
	protected virtual void Eat() {}
}