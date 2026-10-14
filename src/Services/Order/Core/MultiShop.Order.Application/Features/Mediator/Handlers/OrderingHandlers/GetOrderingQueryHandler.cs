using MediatR;
using MultiShop.Order.Application.Features.Mediator.Queries.OrderingQueries;
using MultiShop.Order.Application.Features.Mediator.Results.OrderingResults;
using MultiShop.Order.Application.Interfaces;
using MultiShop.Order.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiShop.Order.Application.Features.Mediator.Handlers.OrderingHandlers
{
    //Burada istek query'den yapılıyor, onun yanıtı queryresult'dan karşılanıyor
    public class GetOrderingQueryHandler : IRequestHandler<GetOrderingQuery, List<GetOrderingQueryResult>>
    {
        private readonly IRepository<Ordering> _repository;

        public GetOrderingQueryHandler(IRepository<Ordering> repository)
        {
            _repository = repository;
        }

        //cancellationToken sayfa yüklenmesini beklerken sayfayı kapattığımızda işlem iptal olsunmu görevi üstleniyor
        public async Task<List<GetOrderingQueryResult>> Handle(GetOrderingQuery request, CancellationToken cancellationToken)
        {
            var values = request.Status is { } status
                ? await _repository.GetListByFilterAsync(x => x.Status == status)
                : await _repository.GetAllAsync();

            return values.OrderByDescending(x => x.OrderDate).Select(x => new GetOrderingQueryResult
            {
                OrderingId = x.OrderingId,
                OrderNumber = x.OrderNumber,
                OrderDate = x.OrderDate,
                TotalPrice = x.TotalPrice,
                UserId = x.UserId,
                AddressId = x.AddressId,
                Status = x.Status,
                FailMessage = x.FailMessage,
                TrackingNumber = x.TrackingNumber,
                CargoCompanyName = x.CargoCompanyName
            }).ToList();
        }
    }
}
