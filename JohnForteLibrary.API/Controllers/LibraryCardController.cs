using CSharpFunctionalExtensions;
using JohnForteLibrary.API.Dtos;
using JohnForteLibrary.API.Requests;
using JohnForteLibrary.API.Responses;
using JohnForteLibrary.Domain;
using JohnForteLibrary.Domain.Enums;
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
    public class LibraryCardController : ControllerBase
    {
        private IWritableRepo<Patron> _writablePatronsRepo;
        private IReadableRepo<Patron> _patronsRepo;

        public LibraryCardController(IWritableRepo<Patron> writablePatronRepo, IReadableRepo<Patron> patronsRepo)
        {
            _writablePatronsRepo = writablePatronRepo;
            _patronsRepo = patronsRepo;
        }

        [HttpPost]
        public async Task<IActionResult> AddLibraryCard(AddLibraryCardRequest request)
        {
            var name = PersonName.Create(request.FirstName, request.LastName);
            var address = Address.Create(request.StreetAddress, request.City, (State) Enum.Parse(typeof(State), request.State), request.ZipCode);
            var email = EmailAddress.Create(request.Email);
            var phone = PhoneNumber.Create(request.PhoneNumber);

            var creatingLibraryCardResult = Result.Combine(name, address, email, phone);

            if (creatingLibraryCardResult.IsFailure) throw new ArgumentException(creatingLibraryCardResult.Error);                        

            var found = true;
            string cardNumber = "";

            while (found)
            {               
                var random = new Random();

                for (int i = 0; i < 14; i++)
                    cardNumber += random.Next(0, 9).ToString();

                var patronList = await _patronsRepo.FindBySpecification(new CardNumberExistsSpecification(cardNumber));

                if (patronList.Count < 1) found = false;
            }

            var libraryCard = new LibraryCard(cardNumber);

            var patronToAdd = new Patron(name.Value, address.Value, phone.Value, email.Value, libraryCard);

            var addedPatron = await _writablePatronsRepo.Add(patronToAdd);

            var response = new AddLibraryCardResponse
            {
                AddedLibraryCard = MapCard(addedPatron)
            };
            return Ok(response);
        }

        private CardDto MapCard(Patron patron)
        {
            if (patron != null)
            {
                return new CardDto
                {
                    Name = patron.Name,
                    cardNumber = patron.Card.CardNumber
                };
            }
            return null;
        }
    }
}