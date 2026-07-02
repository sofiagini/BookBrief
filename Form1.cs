using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BookBriefApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
           
            string query = txtSearch.Text.Trim();

            if (string.IsNullOrEmpty(query))
            {
                MessageBox.Show("Введите название книги для поиска!");
                return;
            }

            btnSearch.Enabled = false;
            btnSearch.Text = "Загрузка...";

           
            BookApiService api = new BookApiService();

            
            Book result = await api.SearchBookAsync(query);

            
            if (result != null)
            {
                lblTitle.Text = "Название: " + result.Title;
                lblAuthor.Text = "Автор: " + result.Author;
                lblRating.Text = "Рейтинг: " + result.Rating;
                rtbSummary.Text = result.Description;
            }
            else
            {
                lblTitle.Text = "Название: -";
                lblAuthor.Text = "Автор: -";
                lblRating.Text = "Рейтинг: -";
                rtbSummary.Text = "Книга не найдена в базе данных Google. Попробуйте ввести более точное название.";
            }

            btnSearch.Enabled = true;
            btnSearch.Text = "Найти книгу";
        }
    }
}