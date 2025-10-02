namespace WiredBrainCoffee.ConsoleApp.Model
{
    public class Person
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public override string ToString()
        {
            return $"FirstName:{FirstName}; LastName:{LastName}";
        }
    }
}