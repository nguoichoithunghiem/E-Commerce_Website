namespace demo.Models.ViewModels
{
    public class ProductDetailsWithQAVM
    {
        public ProductModel ProductDetails { get; set; }
        public List<QuestionModel> Questions { get; set; }
        public List<AnswerModel> Answers { get; set; }
    }
}
