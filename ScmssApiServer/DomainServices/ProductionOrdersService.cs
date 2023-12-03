﻿using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ScmssApiServer.Data;
using ScmssApiServer.DomainExceptions;
using ScmssApiServer.DTOs;
using ScmssApiServer.IDomainServices;
using ScmssApiServer.Models;

namespace ScmssApiServer.DomainServices
{
    public class ProductionOrdersService : IProductionOrdersService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;

        public ProductionOrdersService(AppDbContext dbContext,
                                       IMapper mapper,
                                       UserManager<User> userManager)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<ProductionOrderEventDto> AddManualEventAsync(
            int orderId,
            ProductionOrderEventCreateDto dto)
        {
            ProductionOrder? order = await _dbContext.ProductionOrders
                .Include(i => i.Events)
                .FirstOrDefaultAsync(i => i.Id == orderId);
            if (order == null)
            {
                throw new EntityNotFoundException();
            }

            ProductionOrderEvent orderEvent = order.AddManualEvent(dto.Type, dto.Location, dto.Message);

            await _dbContext.SaveChangesAsync();
            return _mapper.Map<ProductionOrderEventDto>(orderEvent);
        }

        public async Task<ProductionOrderDto> CreateAsync(OrderCreateDto<OrderItemInputDto> dto, string userId)
        {
            User user = await _userManager.Users.Include(i => i.ProductionFacility)
                                                .FirstAsync(x => x.Id == userId);
            if (user.ProductionFacility == null)
            {
                throw new InvalidDomainOperationException(
                        "User needs to belong to a production facility " +
                        "to create a production order."
                    );
            }

            var order = new ProductionOrder
            {
                ProductionFacility = user.ProductionFacility,
                CreateUserId = userId,
                CreateUser = user,
            };

            order.AddItems(await GetOrderItemsFromDtos(dto.Items));
            order.Begin(userId);

            _dbContext.ProductionOrders.Add(order);
            await _dbContext.SaveChangesAsync();
            return _mapper.Map<ProductionOrderDto>(order);
        }

        public async Task<ProductionOrderDto?> GetAsync(int id)
        {
            ProductionOrder? order = await _dbContext.ProductionOrders
                .Include(i => i.Items).ThenInclude(i => i.Product)
                .Include(i => i.ProductionFacility)
                .Include(i => i.Events)
                .Include(i => i.CreateUser)
                .Include(i => i.ApproveProductionManager)
                .Include(i => i.EndUser)
                .FirstOrDefaultAsync(i => i.Id == id);
            return _mapper.Map<ProductionOrderDto?>(order);
        }

        public async Task<IList<ProductionOrderDto>> GetManyAsync()
        {
            IList<ProductionOrder> orders = await _dbContext.ProductionOrders
                .Include(i => i.ProductionFacility)
                .Include(i => i.CreateUser)
                .Include(i => i.ApproveProductionManager)
                .Include(i => i.EndUser)
                .ToListAsync();
            return _mapper.Map<IList<ProductionOrderDto>>(orders);
        }

        public async Task<ProductionOrderDto> UpdateAsync(int id, ProductionOrderUpdateDto dto, string userId)
        {
            ProductionOrder? order = await _dbContext.ProductionOrders
                .Include(i => i.Items).ThenInclude(i => i.Product)
                .Include(i => i.ProductionFacility)
                .Include(i => i.Events)
                .Include(i => i.CreateUser)
                .Include(i => i.ApproveProductionManager)
                .Include(i => i.EndUser)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (order == null)
            {
                throw new EntityNotFoundException();
            }

            if (dto.Items != null)
            {
                _dbContext.RemoveRange(order.Items);
                order.AddItems(await GetOrderItemsFromDtos(dto.Items));
            }

            switch (dto.Status)
            {
                case OrderStatusSelection.Executing:
                    order.StartExecution();
                    break;

                case OrderStatusSelection.WaitingAcceptance:
                    order.FinishExecution();
                    break;

                case OrderStatusSelection.Completed:
                    order.Complete(userId);
                    break;

                case OrderStatusSelection.Canceled:
                    if (dto.Problem == null)
                    {
                        throw new InvalidDomainOperationException(
                                "Cannot cancel an order without a problem."
                            );
                    }
                    order.Cancel(userId, dto.Problem);
                    break;

                case OrderStatusSelection.Returned:
                    if (dto.Problem == null)
                    {
                        throw new InvalidDomainOperationException(
                                "Cannot return an order without a problem."
                            );
                    }
                    order.Return(userId, dto.Problem);
                    break;
            }

            switch (dto.ApprovalStatus)
            {
                case ApprovalStatusOption.Approved:
                    order.Approve(userId);
                    break;

                case ApprovalStatusOption.Rejected:
                    if (dto.Problem == null)
                    {
                        throw new InvalidDomainOperationException(
                                "Cannot reject an order without a problem."
                            );
                    }
                    order.Reject(userId, dto.Problem);
                    break;
            }

            await _dbContext.SaveChangesAsync();
            return _mapper.Map<ProductionOrderDto>(order);
        }

        public async Task<ProductionOrderEventDto> UpdateEventAsync(int id, int orderId, OrderEventUpdateDto dto)
        {
            ProductionOrder? order = await _dbContext.ProductionOrders
                .Include(i => i.Events)
                .FirstOrDefaultAsync(i => i.Id == orderId);
            if (order == null)
            {
                throw new EntityNotFoundException();
            }

            ProductionOrderEvent orderEvent = order.UpdateEvent(id, dto.Message, dto.Location);

            await _dbContext.SaveChangesAsync();
            return _mapper.Map<ProductionOrderEventDto>(orderEvent);
        }

        private async Task<IList<ProductionOrderItem>> GetOrderItemsFromDtos(IEnumerable<OrderItemInputDto> dtos)
        {
            IList<int> productIds = dtos.Select(i => i.ItemId).ToList();
            IDictionary<int, Product> products = await _dbContext
                .Products
                .Include(i => i.SupplyCostItems)
                .ThenInclude(i => i.Supply)
                .Where(i => productIds.Contains(i.Id))
                .ToDictionaryAsync(i => i.Id);

            return dtos.Select(
                dto => new ProductionOrderItem
                {
                    ItemId = dto.ItemId,
                    Unit = products[dto.ItemId].Unit,
                    UnitValue = products[dto.ItemId].Price,
                    UnitCost = products[dto.ItemId].Cost,
                    Quantity = dto.Quantity
                }).ToList();
        }
    }
}
