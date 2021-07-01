using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace JohnForteLibrary.Domain
{
    public class LibraryCard : ValueObject
    {
        //public List<Book> CheckedOutBooks { get; private set; } = new List<Book>();
        public string CardNumber { get; private set; }

        protected LibraryCard() { }

        public LibraryCard(string cardNumber)
        {
            CardNumber = cardNumber;
            
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return CardNumber;
        }
        //public void addCheckedOutBook(Book book)
        //{
        //    CheckedOutBooks.Add(book);
        //}

        //public void removeCheckedOutBook(Book book)
        //{
        //    CheckedOutBooks.Remove(book);
        //}
    }
}
