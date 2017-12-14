using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfCats
{
    static class CatServer
    {
        public static string Cutespaw { get; set; } = "http://www.cutestpaw.com/tag/cats/";
        public static string Pexels { get; set; } = "https://www.pexels.com/search/cat/";
        public static Dictionary<string, string> Selectors { get; set; } = new Dictionary<string, string>
        {
            {"http://www.cutestpaw.com/tag/cats/", "img" },
            { "https://www.pexels.com/search/cat/", "img.photo-item__img" }
        };
    }
}
