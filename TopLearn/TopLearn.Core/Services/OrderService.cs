using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TopLearn.Core.Convertors;
using TopLearn.Core.DTOs.Order;
using TopLearn.Core.Services.Interfaces;
using TopLearn.DataLayer.Context;
using TopLearn.DataLayer.Entities.Course;
using TopLearn.DataLayer.Entities.Order;
using TopLearn.DataLayer.Entities.User;
using TopLearn.DataLayer.Entities.Wallet;

namespace TopLearn.Core.Services
{
    public class OrderService : IOrderService
    {
        private TopLearnContext _context;
        private IUserService _userService;
        public OrderService(TopLearnContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public int AddOrder(string userName, int courseId)
        {
            int userId = _userService.GetUserIdByUserName(userName);
            //int orderId = 0;

            Order order = _context.Orders
                .FirstOrDefault(o => o.UserId == userId && !o.IsFinaly);

            var course = _context.Courses.Find(courseId);

            if (order == null)
            {
                order = new Order()
                {
                    UserId = userId,
                    IsFinaly = false,
                    CreateDate = DateTime.Now,
                    OrderSum = course.CoursePrice,
                    OrderDetails = new List<OrderDetail>()
                    {
                        new OrderDetail()
                        {
                            CourseId = courseId,
                            Count = 1,
                            Price = course.CoursePrice
                        }
                    }
                };
                _context.Orders.Add(order);
                _context.SaveChanges();
            }
            else
            {
                OrderDetail detail =
                    _context.OrderDetails.FirstOrDefault(d => d.OrderId == order.OrderId && d.CourseId == courseId);
                if (detail != null)
                {
                    detail.Count += 1;
                    _context.OrderDetails.Update(detail);
                }
                else
                {
                    detail = new OrderDetail()
                    {
                        OrderId = order.OrderId,
                        Count = 1,
                        CourseId = courseId,
                        Price = course.CoursePrice
                    };
                    _context.OrderDetails.Add(detail);
                }
                _context.SaveChanges();
                UpdatePriceOrder(order.OrderId);
            }
           
            
            return order.OrderId;
        }

        public bool FinalyOrder(string userName,int orderId)
        {
            int userId = _userService.GetUserIdByUserName(userName);
            var order = _context.Orders.Include(o => o.OrderDetails).ThenInclude(od => od.Course)
                .FirstOrDefault(o => o.OrderId == orderId && o.UserId==userId);

            if (order == null || order.IsFinaly)
            {
                return false;
            }

            if (_userService.BalanceUserWallet(userName) >= order.OrderSum)
            {
                order.IsFinaly = true;
                _userService.AddWallet(new Wallet()
                {
                    Amount = order.OrderSum,
                    CreateDate = DateTime.Now,
                    IsPay = true,
                    Description = "فاکتور شماره #" + order.OrderId,
                    UserId = userId,
                    TypeId = 2
                });
                _context.Orders.Update(order);
                foreach (var detail in order.OrderDetails)
                {
                    _context.UserCourses.Add(new UserCourse()
                    {
                        CourseId = detail.CourseId,
                        UserId = userId
                    });
                }
                _context.SaveChanges();
                return true;
            }

            return false;
        }

        public List<Order> GetUserOrders(string userName)
        {
            int userId = _userService.GetUserIdByUserName(userName);

            return _context.Orders.Where(o => o.UserId == userId).ToList();
        }

        public Order GetOrderForUserPanel(string userName, int orderId)
        {
            int userId = _userService.GetUserIdByUserName(userName);

            return _context.Orders.Include(o => o.OrderDetails).ThenInclude(od=>od.Course)
                .FirstOrDefault(o=>o.UserId==userId&&o.OrderId==orderId);
        }

        public void UpdatePriceOrder(int orderId)
        {
            var order = _context.Orders.Find(orderId);
            order.OrderSum = _context.OrderDetails.Where(d => d.OrderId == orderId).Sum(d => d.Price*d.Count);
            _context.Orders.Update(order);
            _context.SaveChanges();
        }

        public int DeleteOrderDetail(int detailId, int orderId)
        {
            int res = 0;
            var orderDetail = getOrderDetailById(detailId);
            orderDetail.IsDelete = true;
            _context.SaveChanges();
            if (!_context.OrderDetails.Any(od => od.OrderId == orderId))
            {
                var order = GetOrderById(orderId);
                order.IsDelete = true;
                _context.SaveChanges();
                res = 1;
            }
            else
            {

                UpdatePriceOrder(orderId);
                _context.SaveChanges();
            }



            return res;
        }

        public DiscountUseType UseDiscount(int orderId,string code)
        {
            var discount = _context.Discounts.SingleOrDefault(d => d.DiscountCode == code);

            ;
            DateTime dateNow = Convert.ToDateTime(DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day);

            if (discount == null)
                return DiscountUseType.NotFound;

            if (discount.StartDate != null && discount.StartDate > dateNow)
                return DiscountUseType.ExpierDate;
            if (discount.EndDate != null && discount.EndDate <= dateNow)
                return DiscountUseType.ExpierDate;

            if (discount.DiscountCode != null && discount.UsableCount < 1)
                return DiscountUseType.Finished;

            var order = GetOrderById(orderId);

            if (_context.UserDiscountCodes.Any(d => d.UserId == order.UserId && d.DiscountId == discount.DiscountId))
                return DiscountUseType.UserUsed;


            int percent = (order.OrderSum * discount.DiscountPercent) / 100;
            order.OrderSum = order.OrderSum-percent;
            UpdateOrder(order);

            if (discount.UsableCount != null)
            {
                discount.UsableCount -= 1;
            }

            _context.Discounts.Update(discount);
            _context.UserDiscountCodes.Add(new UserDiscountCode()
            {
                UserId = order.UserId,
                DiscountId = discount.DiscountId
            });
            _context.SaveChanges();

            return DiscountUseType.Success;
        }

        public Order GetOrderById(int orderId)
        {
            return _context.Orders.Find(orderId);
        }

        public void UpdateOrder(Order order)
        {
            _context.Orders.Update(order);
            _context.SaveChanges();
        }

        public void AddDiscount(Discount discount)
        {
            _context.Discounts.Add(discount);
            _context.SaveChanges();
        }

        public List<Discount> GetAllDiscounts()
        {
            return _context.Discounts.ToList();
        }

        public Discount GetDiscountById(int id)
        {
            return _context.Discounts.Find(id);
        }

        public void UpdateDiscount(Discount discount)
        {
            _context.Discounts.Update(discount);
            _context.SaveChanges();
        }

        public bool IsExistCode(string code)
        {
            return _context.Discounts.Any(d => d.DiscountCode == code);
        }

        public bool IsUserInCourse(string userName, int courseId)
        {
            int userId = _userService.GetUserIdByUserName(userName);

            return _context.UserCourses.Any(c => c.UserId == userId && c.CourseId == courseId);
        }

        public void EditCountOrder(int detailId, int orderId, int count)
        {
            var orderDetail = getOrderDetailById(detailId);
            orderDetail.Count = count;
            _context.SaveChanges();

            UpdatePriceOrder(orderId);

            _context.SaveChanges();
        }

        public OrderDetail getOrderDetailById(int detailId)
        {
           return _context.OrderDetails.Find(detailId);
        }
    }
}
