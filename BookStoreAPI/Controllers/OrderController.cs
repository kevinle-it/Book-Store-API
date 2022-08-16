using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStoreAPI.Data;
using BookStoreAPI.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Net;
using BookStoreAPI.Dto.User.Response;
using BookStoreAPI.Dto.Order.Response;
using BookStoreAPI.Dto.Order.Request;
using BookStoreAPI.Dto.Cart.Response;

namespace BookStoreAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly BookStoreContext _context;

        public OrderController(BookStoreContext context)
        {
            _context = context;
        }

        // GET: api/Order
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderResponse>>> GetOrders()
        {
            try
            {
                var orders = await _context.Orders
                    .Where(order => order.UserId == GetUserId())
                    .ToListAsync();

                if (orders == null)
                {
                    return Ok("Orders not found.");
                }

                return Ok(MapOrdersToResponse(orders));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // POST: api/Order
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<OrderResponse>> PostOrder([FromBody] OrderDto model)
        {
            try
            {
                Order orderToAdd = new()
                {
                    UserId = GetUserId(),
                    TotalPrice = 0,
                    Date = DateTime.UtcNow
                };
                Order addedOrder = _context.Orders.Add(orderToAdd).Entity;

                List<BookOrder> listBooksInCurrentOrder = new();
                List<Book> listBooksToUpdateQuantity = new();
                foreach (var requestedBook in model.ListBooks)
                {
                    var validateBookAndQuantityResult = await AssertBookAndQuantityIsValid(
                            requestedBook.BookId,
                            requestedBook.Quantity
                        );

                    Book currentBook;
                    switch (validateBookAndQuantityResult)
                    {
                        case OkObjectResult ok:
                            currentBook = (Book)ok.Value;
                            break;
                        default:
                            return validateBookAndQuantityResult;
                    }
                    listBooksInCurrentOrder.Add(new BookOrder
                    {
                        OrderId = addedOrder.OrderId,
                        BookId = requestedBook.BookId,
                        Quantity = requestedBook.Quantity,
                        Subtotal = (decimal)(currentBook.Price * requestedBook.Quantity),
                    });
                    // Remove book quantity holding by this order in available stock
                    currentBook.Quantity -= requestedBook.Quantity;
                    listBooksToUpdateQuantity.Add(currentBook);
                }
                // All books are valid
                // => Officially save addedOrder to DB to have real OrderId to
                // work on
                await _context.SaveChangesAsync();

                // Re-update OrderId in list books of current addedOrder
                listBooksInCurrentOrder.ForEach(book => book.OrderId = addedOrder.OrderId);
                await _context.BookOrders.AddRangeAsync(listBooksInCurrentOrder);

                decimal totalPrice = 0;
                listBooksInCurrentOrder.ForEach(book => totalPrice += book.Subtotal);

                addedOrder.TotalPrice = totalPrice;
                _context.Entry(addedOrder).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                    listBooksToUpdateQuantity.ForEach(book =>
                    {
                        _context.Entry(book).State = EntityState.Modified;
                    });
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    // Cannot insert complete order details
                    // => Remove it for safe
                    _context.Orders.Remove(addedOrder);
                    await _context.SaveChangesAsync();
                    return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
                }

                return Ok(MapOrderToResponse(addedOrder));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private string GetUserId()
        {
            return ((ClaimsIdentity)User.Identity).Claims.FirstOrDefault().Value;
        }

        private async Task<ActionResult> AssertBookAndQuantityIsValid(int bookId, int newQuantity)
        {
            var currentBook = await _context.Books
                    .SingleOrDefaultAsync(b => b.BookId == bookId);
            if (currentBook == null)
            {
                return NotFound("Book not found.");
            }

            var availableStock = currentBook.Quantity;
            if (newQuantity > availableStock)
            {
                return BadRequest(
                        $"Requested quantity exceeded available stock " +
                        $"of Book Title: {currentBook.Title}. " +
                        $"Maximum quantity is: {availableStock}"
                    );
            }

            return Ok(currentBook);
        }

        private List<OrderResponse> MapOrdersToResponse(List<Order> orders)
        {
            return orders.Select(order => MapOrderToResponse(order)).ToList();
        }

        private OrderResponse MapOrderToResponse(Order order)
        {
            // Disable Lazy Loading by explicitly loading related entities into order object
            _context.Entry(order)
                    .Reference(c => c.Owner)
                    .Load();
            _context.Entry(order)
                .Collection(c => c.ListBooks)
                .Query()
                .Include("CurrentBook")
                .Load();

            User owner = order.Owner;
            List<BookOrder> listBooks = order.ListBooks.ToList();

            return new OrderResponse
            {
                OrderId = order.OrderId,
                Owner = new UserResponse
                {
                    Id = order.UserId,
                    Email = owner.Email,
                    FullName = owner.FullName,
                    Address = owner.Address,
                    City = owner.City,
                    State = owner.State,
                    PostalCode = owner.PostalCode,
                    PhoneNumber = owner.PhoneNumber,
                },
                ListBooks = listBooks.Select(book => new BookOrderResponse
                {
                    BookId = book.BookId,
                    Title = book.CurrentBook.Title,
                    Category = book.CurrentBook.Category,
                    Author = book.CurrentBook.Author,
                    Price = book.CurrentBook.Price,
                    Summary = book.CurrentBook.Summary,
                    StockAvailable = book.CurrentBook.Quantity,
                    CoverImageURL = book.CurrentBook.CoverImageURL,
                    CoordinateX = book.CurrentBook.CoordinateX,
                    CoordinateY = book.CurrentBook.CoordinateY,
                    Quantity = book.Quantity,
                    Subtotal = book.Subtotal,
                }).ToList(),
                TotalPrice = order.TotalPrice,
                Date = order.Date,
            };
        }
    }
}
