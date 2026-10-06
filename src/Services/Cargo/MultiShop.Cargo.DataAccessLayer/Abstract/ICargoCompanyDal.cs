using MultiShop.Cargo.EntityLayer.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiShop.Cargo.DataAccessLayer.Abstract
{
    //Burada CargoCompany sınıfı için IGenericDaldan miras aldı.
    public interface ICargoCompanyDal : IGenericDal<CargoCompany>
    {
    }
}
