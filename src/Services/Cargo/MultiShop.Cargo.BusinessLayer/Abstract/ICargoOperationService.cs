using MultiShop.Cargo.EntityLayer.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiShop.Cargo.BusinessLayer.Abstract
{
    //IGenericService Sınıfından CargoOperation için miras al.
    public interface ICargoOperationService : IGenericService<CargoOperation>
    {
    }
}
