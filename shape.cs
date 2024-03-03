using System;

abstract class Shape
{
    private static int nextSerialNumber = 1;

    public int SerialNumber { get; private set; }
    public double XAxis { get; set; }
    public double YAxis { get; set; }

    public Shape(double xAxis, double yAxis)
    {
        SerialNumber = nextSerialNumber++;
        XAxis = xAxis;
        YAxis = yAxis;
    }

    public abstract double Area();
    public abstract double Perimeter();
    public abstract string ClassName();
}

class Rectangle : Shape
{
    public Rectangle(double length, double width) : base(length, width) { }

    public override double Area()
    {
        return XAxis * YAxis;
    }

    public override double Perimeter()
    {
        return 2 * (XAxis + YAxis);
    }

    public override string ClassName()
    {
        return "Rectangle";
    }
}

class Oval : Shape
{
    public Oval(double majorAxis, double minorAxis) : base(majorAxis, minorAxis) { }

    public override double Area()
    {
        return Math.PI * XAxis * YAxis;
    }

    public override double Perimeter()
    {
        double a = Math.Max(XAxis, YAxis);
        double b = Math.Min(XAxis, YAxis);

        return 2 * Math.PI * Math.Sqrt((a * a + b * b) / 2);
    }

    public override string ClassName()
    {
        return "Oval";
    }
}

class Circle : Oval
{
    public Circle(double radius) : base(2 * radius, 2 * radius) { }
}

class Program
{
    static double GetUserInput(string prompt)
    {
        double value;
        while (true)
        {
            Console.Write(prompt);
            if (!double.TryParse(Console.ReadLine(), out value) || value <= 0)
            {
                Console.WriteLine("Error!! Please enter a valid positive numeric value.");
                continue;
            }
            return value;
        }
    }

    static void Main(string[] args)
    {
        Console.WriteLine("|=================================================================|");
        Console.WriteLine("OOP CONCEPT: SHAPE");
        Console.WriteLine("|=================================================================|");

        double length, width, majorAxis, minorAxis, radius;

        length = GetUserInput("Please enter the Rectangle Length (in cm): ");
        width = GetUserInput("Please enter the Rectangle Width (in cm): ");

        Rectangle rectangle = new Rectangle(length, width);

        majorAxis = GetUserInput("Please enter the Oval Major Axis (in cm): ");
        minorAxis = GetUserInput("Please enter the Oval Minor Axis (in cm): ");

        Oval oval = new Oval(majorAxis, minorAxis);

        radius = GetUserInput("Please enter the Circle Radius (in cm): ");

        Circle circle = new Circle(radius);

        Console.WriteLine("|=================================================================|");
        Console.WriteLine("RESULT");
        Console.WriteLine("|=================================================================|");

        Console.WriteLine($"Shape Name \t\t\t: {rectangle.ClassName()}");
        Console.WriteLine($"Serial Number \t\t\t: {rectangle.SerialNumber}");
        Console.WriteLine($"Area \t\t\t\t: {rectangle.Area()} Sq.cm");
        Console.WriteLine($"Perimeter \t\t\t: {rectangle.Perimeter()} cm");
        Console.WriteLine("|---------------------------------------------------------------------------------------------|");

        Console.WriteLine($"Shape Name \t\t\t: {oval.ClassName()}");
        Console.WriteLine($"Serial Number \t\t\t: {oval.SerialNumber}");
        Console.WriteLine($"Area \t\t\t\t: {oval.Area()} Sq.cm");
        Console.WriteLine($"Perimeter \t\t\t: {oval.Perimeter()} cm");
        Console.WriteLine("|---------------------------------------------------------------------------------------------|");

        Console.WriteLine($"Shape Name \t\t\t: {circle.ClassName()}");
        Console.WriteLine($"Serial Number \t\t\t: {circle.SerialNumber}");
        Console.WriteLine($"Area \t\t\t\t: {circle.Area()} Sq.cm");
        Console.WriteLine($"Perimeter \t\t\t: {circle.Perimeter()} cm");
        Console.WriteLine("|=================================================================|");
    }
}
