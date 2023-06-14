using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TEST_MVC.Models.ViewModels
{
    public class OrderVM
    {
        public List<OrderM> OrderM { get; set; }
        public List<OrderD> OrderD { get; set; }
    }

    public class OrderVM2
    {
        [DisplayName("訂單編號")]
        public string Id { get; set; }

        [DisplayName("客戶編號")]
        [Required(ErrorMessage = "必填")]
        public int? CustomerId { get; set; }

        [DisplayName("客戶名稱")]
        public string CustomerName { get; set; }

        [DisplayName("訂單總價")]
        [Required(ErrorMessage = "必填")]
        public int? TotalPrice { get; set; }

        [DisplayName("建檔日期")]
        public DateTime? CreactDate { get; set; }

        [DisplayName("建檔人員")]
        public string CreactUser { get; set; }

        [DisplayName("修改日期")]
        public DateTime? EditDate { get; set; }

        [DisplayName("修改人員")]
        public string EditUser { get; set; }

        public int ItemNo { get; set; }

        public int? ProductId { get; set; }
        public string ProductName { get; set; }

        public int? Price { get; set; }

        public int? QTY { get; set; }
        public int? Total { get; set; }
    }
}