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
using System.Net;
using BookStoreAPI.Dto.Cart.Response;
using BookStoreAPI.Dto.User.Response;
using BookStoreAPI.Dto.Cart.Request;
using System.Security.Claims;

namespace BookStoreAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly BookStoreContext _context;

        public CartController(BookStoreContext context)
        {
            _context = context;
        }

        // GET: api/Cart
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Cart>>> GetCarts()
        //{
        //    return await _context.Carts.ToListAsync();
        //}

        // GET: api/Cart/5
        [HttpGet]
        public async Task<ActionResult<CartResponse>> GetCart()
        {
            try
            {
                // Use Include (as JOIN) to enable Eager Loading
                var cart = await _context.Carts
                    .Include("Owner")
                    .Include("ListBooks")
                    .Include("ListBooks.CurrentBook")
                    .Where(cart => cart.UserId == GetUserId())
                    .SingleOrDefaultAsync();

                if (cart == null)
                {
                    return Ok("Cart not found.");
                }

                return MapCartToResponse(cart);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // POST: api/Cart
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPost]
        //public async Task<ActionResult<CartResponse>> PostCart([FromBody] CartDto model)
        //{
        //    try
        //    {
        //        var cart = await _context.Carts.SingleOrDefaultAsync(c => c.UserId == GetUserId());
        //        if (cart == null)
        //        {
        //            Cart cartToAdd = new()
        //            {
        //                UserId = GetUserId(),
        //                TotalPrice = 0,
        //            };
        //            Cart addedCart = _context.Carts.Add(cartToAdd).Entity;
        //            await _context.SaveChangesAsync();

        //            InsertListBooks(addedCart, model);

        //            return Ok(MapCartToResponse(addedCart));
        //        }
        //        else
        //        {
        //            return StatusCode((int)HttpStatusCode.BadRequest, "Cart already exists.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}

        // POST: api/Cart/5/2
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("{bookId}/{quantity}")]
        public async Task<ActionResult<CartResponse>> PostBookToCart(int bookId, int quantity)
        {
            try
            {
                var cart = await _context.Carts
                    .Include("Owner")
                    .Include("ListBooks")
                    .Include("ListBooks.CurrentBook")
                    .SingleOrDefaultAsync(cart => cart.UserId == GetUserId());

                if (cart == null)
                {
                    return NotFound("Cart not found. Create a cart first.");
                }

                var bookCart = cart
                            .ListBooks
                            .SingleOrDefault(lb => lb.BookId == bookId);

                if (bookCart != null)
                {
                    return BadRequest("Book already exists in current cart. Update the book quantity instead.");
                }

                bookCart = new BookCart
                {
                    CartId = cart.CartId,
                    BookId = bookId,
                    Quantity = quantity,
                    Subtotal = (decimal)(bookCart.CurrentBook.Price * quantity),
                };

                _context.BookCarts.Add(bookCart);

                cart.TotalPrice += bookCart.Subtotal;
                _context.Entry(cart).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(MapCartToResponse(cart));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // PUT: api/Cart/5/2
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{bookId}/{quantity}")]
        public async Task<ActionResult<CartResponse>> PutBookInCart(int bookId, int quantity)
        {
            try
            {
                var cart = await _context.Carts
                    .Include("Owner")
                    .Include("ListBooks")
                    .Include("ListBooks.CurrentBook")
                    .SingleOrDefaultAsync(cart => cart.UserId == GetUserId());

                if (cart == null)
                {
                    return NotFound("Cart not found. Create a cart first.");
                }

                var bookCart = cart
                        .ListBooks
                        .SingleOrDefault(lb => lb.BookId == bookId);

                if (bookCart == null)
                {
                    return NotFound("Book not found in current cart. Add the book to cart first.");
                }

                var oldQuantity = bookCart.Quantity;
                bookCart.Quantity = quantity;
                bookCart.Subtotal = (decimal)(bookCart.CurrentBook.Price * quantity);

                _context.Entry(bookCart).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                int diffQuantity = Math.Abs(quantity - oldQuantity);
                decimal diffPrice = (decimal)bookCart.CurrentBook.Price * diffQuantity;
                if (diffQuantity > 0)
                {
                    if (quantity < oldQuantity)
                    {
                        cart.TotalPrice -= diffPrice;
                    }
                    else if (quantity > oldQuantity)
                    {
                        cart.TotalPrice += diffPrice;
                    }
                    _context.Entry(cart).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }

                return Ok(MapCartToResponse(cart));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // DELETE: api/Cart/5
        [HttpDelete("{bookId}")]
        public async Task<ActionResult<CartResponse>> DeleteBookFromCart(int bookId)
        {
            try
            {
                var cart = await _context.Carts
                    .Include("Owner")
                    .Include("ListBooks")
                    .Include("ListBooks.CurrentBook")
                    .SingleOrDefaultAsync(cart => cart.UserId == GetUserId());

                if (cart == null)
                {
                    return NotFound("Cart not found. Create a cart first.");
                }

                var bookCart = cart
                            .ListBooks
                            .SingleOrDefault(lb => lb.BookId == bookId);

                if (bookCart == null)
                {
                    return NotFound("Book not found in current cart. Add the book to cart first.");
                }

                _context.BookCarts.Remove(bookCart);
                await _context.SaveChangesAsync();

                cart.TotalPrice -= bookCart.Subtotal;

                _context.Entry(cart).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(MapCartToResponse(cart));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private bool CartExists(int id)
        {
            return _context.Carts.Any(e => e.CartId == id);
        }

        private string GetUserId()
        {
            return ((ClaimsIdentity)User.Identity).Claims.FirstOrDefault().Value;
        }

        private async void InsertListBooks(Cart addedCart, CartDto model)
        {
            List<BookCart> listBooks = new();
            foreach (var book in model.ListBooks)
            {
                Book existingBook = await _context.Books.SingleOrDefaultAsync(b => b.BookId == book.BookId);
                if (existingBook != null)
                {
                    if (book.Quantity > existingBook.Quantity)
                    {
                        throw new ArgumentException(
                                $"Requested quantity exceeded available stock " +
                                $"of Book Title: {existingBook.Title}. " +
                                $"Maximum quantity is: {existingBook.Quantity}"
                            );
                    }
                    listBooks.Add(new BookCart
                    {
                        CartId = addedCart.CartId,
                        BookId = book.BookId,
                        Quantity = book.Quantity,
                        Subtotal = (decimal)(existingBook.Price * book.Quantity),
                    });
                }
            }

            await _context.BookCarts.AddRangeAsync(listBooks);
            await _context.SaveChangesAsync();

            decimal totalPrice = 0;
            listBooks.ForEach(book => totalPrice += book.Subtotal);

            addedCart.TotalPrice = totalPrice;
            _context.Entry(addedCart).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            // Disable Lazy Loading by explicitly load related entities into addedCart object
            _context.Entry(addedCart).Reference(c => c.Owner).Load();
            _context.Entry(addedCart).Collection(c => c.ListBooks).Load();
        }

        private CartResponse MapCartToResponse(Cart cart)
        {
            User owner = cart.Owner;
            List<BookCart> listBooks = cart.ListBooks.ToList();

            return new CartResponse
            {
                CartId = cart.CartId,
                Owner = new UserResponse
                {
                    Id = cart.UserId,
                    Email = owner.Email,
                    FullName = owner.FullName,
                    Address = owner.Address,
                    City = owner.City,
                    State = owner.State,
                    PostalCode = owner.PostalCode,
                    PhoneNumber = owner.PhoneNumber,
                },
                ListBooks = listBooks.Select(book => new BookCartResponse {
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
                TotalPrice = cart.TotalPrice,
            };
        }
    }
}
