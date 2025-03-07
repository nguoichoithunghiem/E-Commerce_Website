namespace demo.Models
{
    public class QuestionModel
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string Question { get; set; }
        public string UserName { get; set; }
        public DateTime DateAsked { get; set; }
        public string UserRoleId { get; set; } // Lưu thông tin role người hỏi (người dùng)
        public List<AnswerModel> Answers { get; set; }
        // Navigation properties
        public ProductModel Product { get; set; }
    }
}
