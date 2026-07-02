using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookBriefApp
{
    public class Book
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string Rating { get; set; } // Строка, чтобы красиво писать "Нет оценок" или число
    }
}
