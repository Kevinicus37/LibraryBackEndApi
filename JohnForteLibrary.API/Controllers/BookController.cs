using JohnForteLibrary.API.Dtos;
using JohnForteLibrary.API.Requests;
using JohnForteLibrary.API.Responses;
using JohnForteLibrary.Domain;
using JohnForteLibrary.Domain.Repositories;
using JohnForteLibrary.Domain.Specifications;
using JohnForteLibrary.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JohnForteLibrary.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private IReadableRepo<Book> _bookRepo;
        private IWritableRepo<Book> _writableBooksRepo;
        private IReadableRepo<Author> _authorRepo;
        private IReadableRepo<Patron> _libraryCardRepo;

        public BookController(IReadableRepo<Book> bookRepo, IWritableRepo<Book> writableRepo, IReadableRepo<Author> authorRepo, IReadableRepo<Patron> libraryCardRepo)
        {
            _bookRepo = bookRepo;
            _writableBooksRepo = writableRepo;
            _authorRepo = authorRepo;
            _libraryCardRepo = libraryCardRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBooks([FromQuery] string title)
        {
            var books =
                string.IsNullOrEmpty(title) ?
                await _bookRepo.FindAll() :
                await _bookRepo.FindBySpecification(new BookByTitleSpecification(title));

            var bookDtos = books.Select(x => MapBook(x)).ToList();

            var response = new GetAllBooksResponse
            {
                Books = bookDtos
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            var book = await _bookRepo.FindById(id);

            if (book == null)
            {
                return NotFound($"There is no book with id {id}");
            }

            var response = new GetBookByIdResponse
            {
                Book = MapBook(book)
            };
            return Ok(response);
        }

        [Route("[controller]/available")]
        [HttpGet]
        public async Task<IActionResult> GetAllAvailable()
        {

            var books = await _bookRepo.FindBySpecification(new BooksByAvailabilitySpecification());

            var bookDtos = books.Select(x => MapBook(x)).ToList();

            var response = new GetAllAvailableResponse
            {
                Books = bookDtos
            };
            return Ok(response);
        }

        [Route("[controller]/CheckedOut")]
        [HttpGet]
        public async Task<IActionResult> GetAllCheckedOut([FromQuery]string cardnumber)
        {
            var borrowers = await _libraryCardRepo.FindBySpecification(new CardNumberExistsSpecification(cardnumber));

            if (borrowers.Count < 1)
            {

                return NotFound($"There is no library card matching the number {cardnumber}");
            }

            var books = await _bookRepo.FindBySpecification(new BooksByPatronIdSpecification(borrowers[0].Id));

            var bookDtos = books.Select(x => MapBook(x)).ToList();

            var response = new GetAllAvailableResponse
            {
                Books = bookDtos
            };
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewBook(AddBookRequest request)
        {
            List<Author> finalAuthors = new List<Author>();
            foreach (string a in request.Authors)
            {
                if (!string.IsNullOrEmpty(a))
                {
                    List<Author> existingAuthors = await _authorRepo.FindBySpecification(new AuthorByNameSpecification(a));

                    if (existingAuthors.Count > 0)
                    {
                        finalAuthors.Add(existingAuthors[0]);
                    }
                    else
                    {
                        Author author = new Author(a);
                        finalAuthors.Add(author);
                    }
                }
            }


            var isbn = ISBN.Create(request.ISBN);

            if (isbn.IsFailure) throw new ArgumentException(isbn.Error);

            var bookToAdd = new Book(request.Title, finalAuthors, isbn.Value, request.PublishedYear);

            var addedBook = await _writableBooksRepo.Add(bookToAdd);

            var response = new AddBookResponse
            {
                AddedBook = MapBook(addedBook)
            };
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _bookRepo.FindById(id);

            if (book == null)
            {
                return NotFound($"There is no book with id {id}");
            }

            await _writableBooksRepo.Delete(book);

            return Ok();
        }


        [Route("[controller]/CheckOut")]
        [HttpPut]
        public async Task<IActionResult> CheckoutBook(CheckoutBookRequest request)
        {
            var bookToCheckout = await _bookRepo.FindById(request.BookId);

            if (bookToCheckout == null)
            {
                return NotFound($"There is no book with id {request.BookId}");
            }

            var borrowers = await _libraryCardRepo.FindBySpecification(new CardNumberExistsSpecification(request.CardNumber));

            if (borrowers.Count < 1) {

                return NotFound($"There is no library card matching the number {request.CardNumber}");
            }

            bookToCheckout.CheckoutBook(borrowers[0]);

            await _writableBooksRepo.Update(bookToCheckout);

            var due = (DateTime)bookToCheckout.DueDate;

            var response = new CheckoutBookResponse
            {
                DueDate = due.ToLongDateString()
            };

            return Ok(response); 
        }

        [Route("[controller]/CheckIn")]
        [HttpPut]
        public async Task<IActionResult> CheckInBook(ReturnBookRequest request)
        {
            var bookToCheckIn = await _bookRepo.FindById(request.BookId);

            if (bookToCheckIn == null)
            {
                return NotFound($"There is no book with id {request.BookId}");
            }

            bookToCheckIn.CheckinBook();

            await _writableBooksRepo.Update(bookToCheckIn);

            return Ok();
        }



        private BookDto MapBook(Book book)
        {
            if (book != null)
            {
                return new BookDto
                {
                    BookId = book.Id,
                    Title = book.Title,
                    Authors = book.Authors.Select(x => x.Name).ToList(),
                    ISBN = book.ISBN.Value,
                    PublishedYear = book.PublishedYear
                };
            }
            return null;
        }
    }
}
