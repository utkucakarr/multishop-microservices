using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MultiShop.Catalog.Entities
{
    public class Category
    {
        //Categorynin bir id olduğunu bildirmek için aşağıdaki kolları kullanıyoruz
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string ImageUrl { get; set; }
    }
}
