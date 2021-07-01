using FakeItEasy;
using JohnForteLibrary.Domain;
using JohnForteLibrary.Domain.Enums;
using JohnForteLibrary.Domain.Repositories;
using JohnForteLibrary.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace JohnForteLibrary.UnitTests.Domain
{
    public class When_checking_out_a_book
    {
        [Fact]
        public void With_valid_cardNumber()
        {
            var bookRepo = A.Fake<IWritableRepo<Book>>();
            var book = A.Fake<Book>();

            var patron = new Patron(PersonName.Create("Bob", "Smith").Value, Address.Create("29345 135th St.", "Olathe", State.KS, "66062").Value,
                            PhoneNumber.Create("183-349-2483").Value, EmailAddress.Create("bsmith@gmail.com").Value, new LibraryCard("19385948394839"));
            book.CheckoutBook(patron);

            A.CallTo(() => bookRepo.Update(A<Book>.Ignored)).Returns(Task.FromResult(true));

            var bookToCheck = bookRepo.Update(book);

            Assert.True(bookToCheck.Result);
            A.CallTo(() => bookRepo.Update(A<Book>.Ignored)).MustHaveHappened();
        }
    }
}
