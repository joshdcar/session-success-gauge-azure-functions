
namespace SessionSuccessGaugeFunctions
{
    /// <summary>
    /// POCO that represents the Face Data being returned from 
    /// cognitive services emotion API
    /// </summary>
    public class Face
    {
        public faceRectangle faceRectangle { get; set; }
        public scores scores { get; set; }
    }

    public class faceRectangle
    {
        public int top { get; set; }
        public int left { get; set; }
        public int width { get; set; }
        public int height { get; set; }

    }

    public class scores
    {
        public decimal anger { get; set; }
        public decimal contempt { get; set; }
        public decimal disgust { get; set; }
        public decimal fear { get; set; }
        public decimal happiness { get; set; }
        public decimal neutral { get; set; }
        public decimal sadness { get; set; }
        public decimal surprise { get; set; }
    }
}
