namespace Monochrome.Module.Core.Models
{
    public class ApplicationSetting : BaseModel<string>
    {
        public string Value { get; set; }

        public string DataType { get; set; }
    }
}
