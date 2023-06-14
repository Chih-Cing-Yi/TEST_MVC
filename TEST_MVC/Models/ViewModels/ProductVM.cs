namespace TEST_MVC.Models.ViewModels
{
    public class ProductVM
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? Price { get; set; }

        public int? Stock { get; set; }

        public string ImageUrl { get; set; }

        public int ProductTypeId { get; set; }
        public string ProductTypeName { get; set; }

        public DateTime CreatDate { get; set; }

        public string CreatUser { get; set; }

        public DateTime? EditDate { get; set; }

        public string EditUser { get; set; }
    }
}