using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiShop.DtoLayer.IdentityDtos.UserDtos
{
    // IdentityServer yalnızca bu alanları döner (şifre hash'i gibi alanlar artık gelmiyor).
    public class ResultUserDto
    {
        public string id { get; set; }
        public string userName { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
    }

}
