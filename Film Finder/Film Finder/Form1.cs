using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml;

namespace Film_Finder
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            readData();

          
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            setUpListview(movieList);
            SizeLastColumn(movieList);
            setUpListview(WatchlistView);
            SizeLastColumn(WatchlistView);
        }

        private List<Movie> Movies = new List<Movie>();
        private List<String> suggestions = new List<String>();
        String file = "movies.xml";
        int count = 0;
        private static int range = 350;

        private String[] Genres = { "Action","Adventure","Animation","Biography","Comedy","Crime","Documentary",
                                    "Drama","Family","Fantasy","History","Horror","Music","Musical","Mystery",
                                    "Romance","Sci-Fi","Sport","Thriller","War","Western"};
        private Brush[] color = {Brushes.SlateBlue,Brushes.Green,Brushes.Violet,Brushes.DimGray,Brushes.Orange,Brushes.DeepPink,
                                    Brushes.Brown,Brushes.IndianRed,Brushes.Navy,Brushes.Magenta,Brushes.Chocolate,Brushes.Black,
                                    Brushes.MediumPurple,Brushes.DarkOrchid,Brushes.SteelBlue,Brushes.Red,Brushes.CadetBlue,
                                    Brushes.LimeGreen, Brushes.Blue,Brushes.DarkGoldenrod,Brushes.DarkKhaki};

        char[] AtoZ = {'#' ,'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        private static Boolean allTitle = true;
        private static Boolean allDirector = true;
        private static Boolean allActor = true;
        private static int titleIndex = 0;
        private static int directorIndex = 1;
        private static int actorIndex = 1;
        private static Boolean editing = false;
        private static int selectedindex = 0;
        private static int listselectedindex = 0;

        HashFrequency searchlog = new HashFrequency();

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.CloseReason == CloseReason.WindowsShutDown) return;

            // Confirm user wants to close
            switch (MessageBox.Show(this, "Do you want to save Changes?", "Closing", MessageBoxButtons.YesNoCancel))
            {
                case DialogResult.Cancel:
                    e.Cancel = true;
                    break;
                case DialogResult.Yes:
                    saveData();
                    MessageBox.Show("Changes was saved to " + file);
                    break;
                default:
                    break;
            }
        }
     
        private void update(String text)
        {
            updatelog.Visible = true;
            updatelog.Text = text;    
            timer1.Start();
           
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            updatelog.Visible = false;
            timer1.Stop();
        }
        private void readData()
        {

            XmlDocument xml = new XmlDocument();

           

            xml.Load(file);
            XmlNodeList xnList = xml.SelectNodes("movielist/movie");

            foreach (XmlNode xn in xnList)
            {
                if (xn.Name == "movie")
                {
                    xn.SelectNodes("movie");
                    Movie nMovie = new Movie();
                    count++;
                    List<String> actors = new List<String>();
                    List<String> genres = new List<String>();

                    foreach (XmlNode xnchild in xn)
                    {
                        if (xnchild.NodeType == XmlNodeType.Element)
                        {
                            switch (xnchild.Name)
                            {
                                case "title":
                                    nMovie.Title = xnchild.InnerText;
                                    suggestions.Add(xnchild.InnerText);
                                    break;
                                case "year":
                                    nMovie.Year = xnchild.InnerText;
                                    break;
                                case "actor":
                                     actors.Add(xnchild.InnerText);
                                    suggestions.Add(xnchild.InnerText);
                                    break;
                                case "certification":
                                    nMovie.Certification = xnchild.InnerText;
                                    break;
                                case "rating":
                                    nMovie.Rating = xnchild.InnerText;
                                    break;
                                case "length":
                                    nMovie.Time = xnchild.InnerText;
                                    break;
                                case "genre":
                                    genres.Add(xnchild.InnerText);
                                    break;
                                case "director":
                                    nMovie.Director = xnchild.InnerText;
                                    suggestions.Add(xnchild.InnerText);
                                    break;
                                case "data":
                                    if (xnchild.InnerText.Substring(0, 1) == "1")
                                        nMovie.watchList = true;
                                    break;
                            }
                        }
                    }
                    nMovie.Actor = actors.ToArray();
                    nMovie.Genre = genres.ToArray();

                    ListViewItem item = new ListViewItem(nMovie.Title);
                    item.SubItems.Add(nMovie.Year);
                    item.SubItems.Add(nMovie.Time);
                    item.SubItems.Add(nMovie.Certification);
                    item.SubItems.Add(nMovie.Director);
                    item.SubItems.Add(nMovie.Rating);
                    movieList.Items.Add(item);
                    if (nMovie.watchList)
                    {
                        ListViewItem item2 = new ListViewItem(nMovie.Title);
                        item2.SubItems.Add(nMovie.Year);
                        item2.SubItems.Add(nMovie.Time);
                        item2.SubItems.Add(nMovie.Certification);
                        item2.SubItems.Add(nMovie.Director);
                        item2.SubItems.Add(nMovie.Rating);
                        
                        WatchlistView.Items.Add(item2);
                    }
                    Movies.Add(nMovie);
                   
                }
            }

            var source = new AutoCompleteStringCollection();

            
            source.AddRange(suggestions.ToArray());
            searchBox.AutoCompleteCustomSource = source;
            searchBox.AutoCompleteMode = AutoCompleteMode.Suggest;
            searchBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
            
            DTitle.Text = count + " items";
        }
        private void saveData()
        {

            XmlDocument doc = new XmlDocument();
            doc.Load(file);

            foreach (XmlNode xnode in doc.SelectNodes("movielist"))
                xnode.RemoveAll();

            foreach (Movie m in Movies)
            {
                XmlNode movie = doc.CreateElement("movie");
                XmlNode title = doc.CreateElement("title");
                XmlNode year = doc.CreateElement("year");
                XmlNode length = doc.CreateElement("length");
                XmlNode cert = doc.CreateElement("certification");
                XmlNode rating = doc.CreateElement("rating");
                XmlNode director = doc.CreateElement("director");
               
                
                XmlNode data = doc.CreateElement("data");


                title.InnerText = m.Title;
                year.InnerText = m.Year;
                length.InnerText = m.Time;
                cert.InnerText = m.Certification;
                rating.InnerText = m.Irate + "";
                director.InnerText = m.Director;

                movie.AppendChild(title);
                movie.AppendChild(year);
                movie.AppendChild(length);
                movie.AppendChild(cert);
                movie.AppendChild(rating);
                movie.AppendChild(director);

                int i = 0;
                while (i < m.Genre.Length)
                {
                    if (m.Genre[0] != "")
                    {
                        XmlNode genre = doc.CreateElement("genre");
                        String str = m.Genre[i];
                        genre.InnerText = str;

                        movie.AppendChild(genre);
                    }
                    i++;
                }
                i = 0;
                while (i < m.Actor.Length)
                {
                    if (m.Actor[i] != "")
                    {
                        XmlNode actor = doc.CreateElement("actor");
                        String str = m.Actor[i];
                        actor.InnerText = str;
                        movie.AppendChild(actor);
                    }
                    i++;
                }

                String result = "";
                if (m.watchList)
                    result = "1";
                else
                    result = "0";
                if (m.Like)
                    result += "1";
                else
                    result += "0";

                data.InnerText = result;
                movie.AppendChild(data);
                
                doc.DocumentElement.AppendChild(movie);

            }
            doc.Save(file);
        }
        //list editor -----------------------------
        private void setUpListview(ListView lv)
        {
            ImageList imgList = new ImageList();
            imgList.ImageSize = new Size(1, 32);
            lv.SmallImageList = imgList;

            int i = 0;
            foreach (ListViewItem each in lv.Items)
            {
                if ((i % 2) == 0)
                {
                    each.BackColor = Color.LightGray;
                }
                i++;
            }
        }
        private void SizeLastColumn(ListView lv)
        {
            int x = (lv.Width - 10)/ 15 == 0 ? 1 : (lv.Width - 10) / 15;
            lv.Columns[0].Width = x * 4;
            lv.Columns[1].Width = x * 2;
            lv.Columns[2].Width = x * 2 - 5;
            lv.Columns[3].Width = x * 2;
            lv.Columns[4].Width = x * 2;
            lv.Columns[5].Width = x * 3;
        }

        private void backButtonView_Click(object sender, EventArgs e)
        {
            previewPanel.SendToBack();
        }

        private void movieList_DoubleClick(object sender, EventArgs e)
        {
            int i = 0;
            Boolean done = false;
            foreach (ListViewItem eachItem in movieList.SelectedItems)
            {
                while (i < Movies.Count && !done)
                {
                    if (Movies[i].Title.Equals(eachItem.SubItems[0].Text))
                    {
                        done = true;
                        preview(Movies[i]);
                    }
                    i++;
                }
            }
        }

        //Preview movies
        public void preview(Movie movie)
        {
            previewPanel.BringToFront();
            PreviewTitle.Text = movie.Title;
            previewDirector.Text = movie.Director;
            previewCert.Text = movie.Certification;
            previewRating.Text = movie.Irate + "/" + "10";
            PreviewTime.Text = movie.Time;

            String genres = "";
            String actors = "";
            for (int i = 0; i < movie.Genre.Length && movie.Genre[i] != "" ; i++)
            {
                genres += (i + 1 < movie.Genre.Length) ? movie.Genre[i] + " | "
                    : movie.Genre[i] + ".";
            }
            for (int i = 0; i < movie.Actor.Length && movie.Actor[i] != ""; i++)
            {
                actors += (1 + i < movie.Actor.Length) ? movie.Actor[i] + ", "
                    : movie.Actor[i] + ".";
            }

            previewActors.Text = actors;
            PreviewGenre.Text = genres;

            if (movie.Like)
            {
                previewLike.Image = imageList1.Images[1];
            }
            else
            {
                previewLike.Image = imageList1.Images[0];
            }

            if (movie.watchList)
            {
                prevAddWatchlist.Text = "Remove Watchlist";
            }
            else
            {
                prevAddWatchlist.Text = "Add To WatchList";
            }
        }
        private String TruncateAtWord(String input, int length)
        {
            if (input == null || input.Length < length)
                return input;
            int iNextSpace = input.LastIndexOf(" ", length);
            return string.Format("{0}...", input.Substring(0, (iNextSpace > 0) ? iNextSpace : length).Trim());
        }

        private void PreviewEdit_Click(object sender, EventArgs e)
        {
            Boolean done = false;
            int i = 0;
            foreach (ListViewItem eachItem in movieList.SelectedItems)
            {
               
                while (i < Movies.Count && !done)
                {
                    if (Movies[i].Title.Equals(eachItem.SubItems[0].Text))
                    {
                        editMovie(Movies[i]);
                        selectedindex = i;
                        listselectedindex = movieList.Items.IndexOf(eachItem);
                        editing = true;
                        done = true;
                    }
                    i++;
                }
               
            }
        }
        private void prevAddWatchlist_Click(object sender, EventArgs e)
        {
            prevAddWatchlist.Text = (prevAddWatchlist.Text.Equals("Add To WatchList")) ?
               "Remove Watchlist" : "Add To WatchList";

            Boolean done = false;
            int i = 0;

            foreach (ListViewItem eachItem in movieList.SelectedItems)
            {

                while (i < Movies.Count && !done)
                {
                    Movie nMovie = Movies[i];
                    if (nMovie.Title.Equals(eachItem.SubItems[0].Text))
                    {
                        if (nMovie.watchList)
                        {
                            nMovie.watchList = false;
                            removeFromWatchList(nMovie.Title);
                            update(nMovie.Title + " was removed from Watchlist");
                        }
                        else 
                        {
                            nMovie.watchList = true;
                        }
                        done = true;
                        update(nMovie.Title + " was added from Watchlist");
                        printWatchlist();
                    }
                    i++;
                }
            }
        }
        private void removeWatchlist_Click(object sender, EventArgs e)
        {
            Boolean done = false;
            int i = 0;

            foreach (ListViewItem eachItem in WatchlistView.SelectedItems)
            {

                while (i < Movies.Count && !done)
                {
                    Movie nMovie = Movies[i];
                    if (nMovie.Title.Equals(eachItem.SubItems[0].Text))
                    {
                            nMovie.watchList = false;
                            removeFromWatchList(nMovie.Title);
                            update(nMovie.Title + " was removed from Watchlist");
                        printWatchlist();
                    }
                    i++;
                }
            }
        }
        public void removeFromWatchList(String item)
        {
            foreach (ListViewItem lv in WatchlistView.Items)
            {
                if (lv.SubItems[0].Text == item)
                {
                    WatchlistView.Items.Remove(lv);
                }
            }
        }
        public void printWatchlist()
        {
            foreach (ListViewItem item in WatchlistView.Items)
            {
                WatchlistView.Items.Remove(item);
            }
            foreach (Movie m in Movies)
            {
                if (m.watchList)
                {
                    ListViewItem item = new ListViewItem(m.Title);
                    item.SubItems.Add(m.Year);
                    item.SubItems.Add(m.Time);
                    item.SubItems.Add(m.Certification);
                    item.SubItems.Add(m.Director);
                    item.SubItems.Add(m.Rating);
                    WatchlistView.Items.Add(item);
                }
            }
            setUpListview(WatchlistView);
            SizeLastColumn(WatchlistView);
        }

        private void previewLike_Click(object sender, EventArgs e)
        {
            int i = 0;
            Boolean done = false;
            foreach (ListViewItem eachItem in movieList.SelectedItems)
            {
                while (i < Movies.Count && !done)
                {
                    if (Movies[i].Title.Equals(eachItem.SubItems[0].Text))
                    {
                        done = true;
                        if (Movies[i].Like)
                        {
                            Movies[i].Like = false;
                            previewLike.Image = imageList1.Images[0];
                            
                        }
                        else
                        {
                            Movies[i].Like = true;
                            previewLike.Image = imageList1.Images[1];
                            update(Movies[i].Title + " was Liked");
                        }

                    }
                    i++;
                }
            }
        }
        public void setdetail()
        {
            DActor.Visible = true;
            DTimeLabel.Visible = true;
            Dtime.Visible = true;
            DActorlabel.Visible = true;
            DGenre.Visible = true;
            DGenrelabel.Visible = true;

        }

        private void movieList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int i = 0;
            Boolean done = false;

            foreach (ListViewItem item in movieList.SelectedItems)
            {
                while (i < Movies.Count && !done)
                {
                    if (Movies[i].Title.Equals(item.SubItems[0].Text))
                    {
                        DTitle.Text = TruncateAtWord(Movies[i].Title, (Mdetail.Width / 20));
                        setdetail();
                        String actors = "";
                        foreach (String str in Movies[i].Actor)
                        {
                            actors += str + ", ";
                        }
                        String genres = "";
                        foreach (String str in Movies[i].Genre)
                        {
                            genres += str + ", ";
                        }
                        DActor.Text = TruncateAtWord(actors, (Mdetail.Width / 5) - actors.Length); ;
                        Dtime.Text = Movies[i].Time;
                        DGenre.Text = genres;

                        if (Movies[i].Like)
                            Dlike.Text = char.ConvertFromUtf32(10084);
                        else
                            Dlike.Text = "";
                    }
                    i++;
                }
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            SizeLastColumn(movieList);
            SizeLastColumn(WatchlistView);
            SizeLastColumn(resultlistView);
        }

        private void searchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Lookup(searchBox.Text);
                resultPanel.BringToFront();
            }
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            Lookup(searchBox.Text);
            resultPanel.BringToFront();
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            AddEditPanel.BringToFront();
            AddEditlabel.Text = "Add Movie";
            editing = false;
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            AddEditPanel.BringToFront();
            AddEditlabel.Text = "Edit Movie";

            Boolean done = false;
            int i = 0;
            foreach (ListViewItem eachItem in movieList.SelectedItems)
            {
               
                while (i < Movies.Count && !done)
                {
                    if (Movies[i].Title.Equals(eachItem.SubItems[0].Text))
                    {
                        editMovie(Movies[i]);
                        selectedindex = i;
                        listselectedindex = movieList.Items.IndexOf(eachItem);
                        done = true;
                        editing = true;
                    }
                    i++;
                }
             
            }
        }

        private void editMovie(Movie movie)
        {
            textBox1.Text = movie.Title;
            comboBox1.Text = movie.Year;
            numericUpDown1.Value = movie.Irate;
            textBox2.Text = movie.Time ;
            textBox3.Text = movie.Director ;
            comboBox2.Text = movie.Certification;

            textBox4.Text = movie.Actor[0];

            if(movie.Actor.Length > 1)
                textBox5.Text = movie.Actor[1];
            if (movie.Actor.Length > 2)
                textBox6.Text = movie.Actor[2];
            if (movie.Actor.Length > 3)
                textBox6.Text = movie.Actor[3];
            if (movie.Actor.Length > 4)
                textBox7.Text = movie.Actor[4];

            
            comboBox3.Text = movie.Genre[0];

            if (movie.Genre.Length > 1)
                comboBox4.Text = movie.Genre[1];
            if (movie.Genre.Length > 2)
                comboBox5.Text = movie.Genre[2];
            if (movie.Genre.Length > 3)
                comboBox6.Text = movie.Genre[3];
            if (movie.Genre.Length > 4)
                comboBox7.Text = movie.Genre[4];

            AddEditPanel.BringToFront();
            AddEditlabel.Text = "Edit Movie";
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            Boolean done = false;
            int i = 0;
            foreach (ListViewItem eachItem in movieList.SelectedItems)
            {

                while (i < Movies.Count && !done)
                {
                    if (Movies[i].Equals(eachItem.SubItems[0].Text))
                    {
                        update(Movies[i].Title + " was deleted");
                        Movies.Remove(Movies[i]);//delete item
                        movieList.Items.Remove(eachItem);
                        done = true;
                    }
                    i++;
                }

            }
            movieList.Refresh();
        }

        private void addWatchlistButton_Click(object sender, EventArgs e)
        {
            Boolean done = false;
            int i = 0;

            foreach (ListViewItem eachItem in movieList.SelectedItems)
            {

                while (i < Movies.Count && !done)
                {
                    Movie nMovie = Movies[i];
                    if (nMovie.Title.Equals(eachItem.SubItems[0].Text) && !nMovie.watchList)
                    {
                        nMovie.watchList = true;
                        update(Movies[i].Title + " was added to Watchlist");
                        done = true;
                        printWatchlist();
                    }
                    i++;
                }
            }
        }

        private void likeButton_Click(object sender, EventArgs e)
        {
            int i = 0;
            Boolean done = false;
            foreach (ListViewItem eachItem in movieList.SelectedItems)
            {
                while (i < Movies.Count && !done)
                {
                    if (Movies[i].Title.Equals(eachItem.SubItems[0].Text) && !Movies[i].Like)
                    {
                        done = true;
                        update(Movies[i].Title + " was Liked");
                        Movies[i].Like = true;
                    }
                    i++;
                }
            }
        }

        private void resultlistView_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void resultlistView_DoubleClick(object sender, EventArgs e)
        {
            int i = 0;
            Boolean done = false;
            foreach (ListViewItem eachItem in resultlistView.SelectedItems)
            {
                while (i < Movies.Count && !done)
                {
                    if (Movies[i].Title.Equals(eachItem.SubItems[0].Text))
                    {
                        done = true;
                        preview(Movies[i]);
                    }
                    i++;
                }
            }
        }

        private void backResultButton_Click(object sender, EventArgs e)
        {
            resultPanel.SendToBack();
        }
        public void Lookup(String text)
        {
            foreach (ListViewItem item in resultlistView.Items)
            {
                resultlistView.Items.Remove(item);
            }

            searchlabel.Text = "' " + text + " '";
            Boolean found;
            foreach (Movie m in Movies)
            {
                found = false;
                if (m.Title.ToLower().IndexOf(text.ToLower()) != -1
                    | m.Director.ToLower().IndexOf(text.ToLower()) != -1)
                {
                    found = true;
                }
                foreach (String str in m.Actor)
                {
                    if (str.ToLower().IndexOf(text.ToLower()) != -1)
                    {
                        found = true;
                    }
                }
                if (found)
                {
                    ListViewItem item = new ListViewItem(m.Title);
                    item.SubItems.Add(m.Year);
                    item.SubItems.Add(m.Time);
                    item.SubItems.Add(m.Certification);
                    item.SubItems.Add(m.Director);
                    item.SubItems.Add(m.Rating);
                    
                    resultlistView.Items.Add(item);
                    if(m.Genre[0] !=  null)
                        searchlog.Add(m.Genre[0]);
                    //if(m.Actor[0] != null)
                      //  searchlog.Add(m.Actor[0]);

                }
            }
            setUpListview(resultlistView);
            SizeLastColumn(resultlistView);
            recommendMovie();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            searchBox.Text = "";
        }

        private void AEBackButton_Click(object sender, EventArgs e)
        {
            AddEditPanel.SendToBack();
        }

        private void AECancel_Click(object sender, EventArgs e)
        {
            AddEditPanel.SendToBack();
            clear();
        }

        public void recommendMovie()
        {
            foreach (ListViewItem item in FeaturedlistView.Items)
            {
                FeaturedlistView.Items.Remove(item);
            }
            String [] result = searchlog.list;
            int i = 0;
            Boolean stop = false;
            int count1 = 0, x = 0;
            while (count1 < 30)
            {
                String str = result[x];
                while (i < Movies.Count && !stop && count1 < 30)
                {
                    Movie m = Movies[i];
                    if (!m.watchList && m.Genre[0] == str | m.Actor[0] == str)
                    {
                        ListViewItem item = new ListViewItem(m.Title);
                        /*
                        item.SubItems.Add(m.Year);
                        item.SubItems.Add(m.Time);
                        item.SubItems.Add(m.Certification);
                        item.SubItems.Add(m.Director);
                        item.SubItems.Add(m.Rating);*/
                        FeaturedlistView.Items.Add(item);
                        stop = true;
                        count1++;
                    }
                    i++;
                }
                x += x % result.Length;
                stop = false;
            }
            setupRMovie(FeaturedlistView);
        }
        public void setupRMovie(ListView list)
        {
            foreach (ListViewItem item in list.Items)
            {
                item.ImageIndex = 0;
            }
        }

        private void Form1_Validating(object sender, CancelEventArgs e)
        {
            
        }

        private void AddEditPanel_Enter(object sender, EventArgs e)
        {
            foreach (Control control in this.Controls)
            {
                string toolTip = toolTip1.GetToolTip(control);

                if (toolTip.Length == 0)
                    continue;
                infoProvider.SetError(control, toolTip);
            }
        }
        private void AddEditPanel_Validating(object sender, EventArgs e)
        {
            string toolTip = toolTip1.GetToolTip((Control)sender);
            if (textBox1.Text == "")
            {
                 errorProvider1.SetError(textBox1, toolTip);
               // infoProvider.SetError(textBox1, toolTip);
            }

            if (comboBox1.Text == "")
            {
                 errorProvider1.SetError(textBox1, toolTip);
                //infoProvider.SetError(comboBox1, toolTip);
            }

            if (comboBox2.Text == "")
            {
                 errorProvider1.SetError(textBox1, toolTip);
                //infoProvider.SetError(comboBox2, toolTip);
            }

            if (textBox2.Text == "")
            {
                errorProvider1.SetError(textBox1, toolTip);
               // infoProvider.SetError(textBox2, toolTip);
            }

            if (textBox3.Text == "")
            {
                 errorProvider1.SetError(textBox1, toolTip);
                //infoProvider.SetError(textBox3, toolTip);
            }

            if (comboBox3.Text == "")
            {
                 errorProvider1.SetError(textBox1, toolTip);
                //infoProvider.SetError(comboBox4, toolTip);
            }
        }
        private void AESave_Click(object sender, EventArgs e)
        {
            AddEditPanel_Validating(sender, e);

            if (textBox1.Text.Length != 0 && comboBox1.Text.Length != 0
                && textBox3.Text.Length != 0 && textBox4.Text.Length != 0
                && comboBox3.Text.Length != 0)
            {
                Movie movie = new Movie();
                movie.Title = textBox1.Text;
                movie.Year = comboBox1.Text;
                movie.Rating = numericUpDown1.Value + "";
                movie.Director = textBox3.Text;
                movie.Time = textBox2.Text;
                movie.Certification = comboBox2.Text;

                String[] actor = new String[5];
                String[] genre = new String[5];

                actor[0] = textBox4.Text;

                if (textBox5 != null)
                    actor[1] = textBox5.Text;
                if (textBox6 != null)
                    actor[2] = textBox6.Text;
                if (textBox7 != null)
                    actor[3] = textBox7.Text;
                if (textBox8 != null)
                    actor[4] = textBox8.Text;

                genre[0] = comboBox3.Text;

                if (comboBox4 != null)
                    genre[1] = comboBox4.Text;
                if (comboBox5 != null)
                    genre[2] = comboBox5.Text;
                if (comboBox6 != null)
                    genre[3] = comboBox6.Text;
                if (comboBox7 != null)
                    genre[4] = comboBox7.Text;

                movie.Actor = actor;
                movie.Genre = genre;

              

                if (editing)
                {
                    Movies[selectedindex] = movie;
                    movieList.Items.RemoveAt(listselectedindex);
                }
                else
                {
                    Movies.Add(movie);
                    suggestions.Add(movie.Title);
                    suggestions.Add(movie.Director);
                    suggestions.Add(movie.Actor[0]);
                    update(movie.Title + " was added");
                }

                ListViewItem item = new ListViewItem(movie.Title);
                item.SubItems.Add(movie.Year);
                item.SubItems.Add(movie.Time);
                item.SubItems.Add(movie.Certification);
                item.SubItems.Add(movie.Director);
                item.SubItems.Add(movie.Rating);

                if ((listselectedindex % 2) != 0)
                {
                    item.BackColor = Color.LightGray;
                }

                movieList.Items.Add(item);

                clear();
                var source = new AutoCompleteStringCollection();
                source.AddRange(suggestions.ToArray());
                searchBox.AutoCompleteCustomSource = source;
                searchBox.AutoCompleteMode = AutoCompleteMode.Suggest;
                searchBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
            }
            AddEditPanel.SendToBack();
        }
        private void clear()
        {
            //clear
            textBox1.Text = "";

            comboBox1.Text = "";

            numericUpDown1.Value = 0;

            textBox2.Text = "";

            textBox3.Text = "";
            comboBox2.Text = "";
            textBox4.Text = "";

            textBox5.Text = "";
            textBox6.Text = "";
            textBox7.Text = "";
            textBox8.Text = "";

            comboBox3.Text = "";

            comboBox4.Text = "";
            comboBox5.Text = "";
            comboBox6.Text = "";
            comboBox7.Text = "";
        }
        private void movieGraph_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            int height = (movieGraph.Height * 9)/10;
            int width = (movieGraph.Width * 8)/10;

            Pen pen = new Pen(Color.DarkCyan, 1);

            int upY = height/20;
            int downY = height;
            int frontX = width/20;
            int backX = width;

            //scale
            int scaleX = backX / 23;
            int scaleY = downY / 10;

            //boundary
            int maxYear = 2015;
            int minYear = 1900;
            int minRating = 0;
            int maxRating = 10;

            int count = 0;

            //Draw a box
            g.DrawRectangle(pen, frontX, upY, width, height);

            using (Font myFont = new Font("Arial", (backX / 80) < 11 ? (backX / 80) : 11))
            {
                //Year label (horizontal)
                int tempX = frontX - 7;
                for (int i = minYear; i <= maxYear; i += 5)
                {
                    g.DrawString(i + "", myFont, Brushes.Black, new PointF(tempX, (downY + upY) + 5));
                    tempX += scaleX;
                }

                //Rating label (vertical)
                int tempY = (downY + upY)-7;
                for (int i = minRating; i <= maxRating; i++)
                {
                    g.DrawString(i + "", myFont, Brushes.Black, new PointF(frontX - ((myFont.Size < 11) ? 20 : 35), tempY));
                    tempY -= scaleY;
                }
            }
            //draw scatterplot
            Brush tempBrush;
            Point tempPoint;
            //redefine scale(year) to contain all years from 1930 - 2015 
            scaleX = backX  / (maxYear - minYear);
            foreach (Movie movie in Movies)
            {
                if (filter(movie))
                {
                    if (movie.Iyear >= minYear && movie.Iyear <= maxYear
                        && movie.Irate >= minRating && movie.Irate <= maxRating)
                    {
                        if (movie.Genre.Length > 1)
                        {
                            tempBrush = Brushes.DarkTurquoise;
                        }
                        else
                        {
                            tempBrush = getColor(movie.Genre[0]);
                        }
                        tempPoint = getCordinate((movie.Iyear - minYear), (movie.Irate - minRating), frontX, (downY + upY)-5, scaleX, scaleY);
                        movie.graphPoint = tempPoint;

                        g.FillEllipse(tempBrush, tempPoint.X, tempPoint.Y, scaleY / 8, scaleY / 8);
                        //g.FillRectangle(tempBrush, tempPoint.X, tempPoint.Y, scaleY / 9, scaleY / 13);
                        count++;
                    }
                }
                else
                {
                    movie.graphPoint = new Point(0, 0);
                }
            }
            int x = 0, m = scaleY / 2;

            using (Font myFont = new Font("Arial", scaleY / 7, FontStyle.Bold))
            {
                g.DrawString("Multiple Genre", myFont, Brushes.DarkTurquoise, new PointF((width *10)/9, m));
                foreach (String str in Genres)
                {

                    g.DrawString(str, myFont, color[x], new PointF((width * 10) / 9, m += (scaleY / 3)));
                    x++;
                }
            }
            FilmShown.Text = count + "";
        }
        public Brush getColor(String genre)
        {
            Brush temp = (Brush)(Brushes.Black);
            int i = 0;
            Boolean found = false;
            while (i < Genres.Length && !found)
            {
                if (genre.Equals(Genres[i]))
                {
                    temp = color[i];
                }
                i++;
            }
            return temp;
        }
        public Point getCordinate(int year, int rating, int x, int y, int scaleX, int scaleY)
        {

            return new Point(x + (year * (scaleX)) , y - (rating * scaleY));
        }
        private Boolean filter(Movie movie)
        {
            Boolean result = false;

            //check Iyear
            if (Int32.Parse(YearFrom.Text) <= movie.Iyear && movie.Iyear <= Int32.Parse(YearTo.Text))
            {
                result = true;
            }
            //check rating
            if (RatingGuage.Text == "<=" && result)
            {
                if (movie.Irate <= RatingMeter.Value)
                    result = true && result;
                else
                    result = false;
            }else
                if (RatingGuage.Text == ">=" && result)
                {
                    if (movie.Irate >= RatingMeter.Value)
                        result = true && result;
                    else
                        result = false;
                }
                else
                {
                    if (movie.Irate == RatingMeter.Value)
                        result = true;
                    else
                        result = false;
                }
            //check certification
            if (G.Checked && movie.Certification == "G")
            {
               result = true && result;
            }
            else
            if (PG.Checked && movie.Certification == "PG")
            {
               result = true && result;
            }
            else
            if (PG13.Checked && movie.Certification == "PG-13")
            {
                result = true && result;
            }
            else
            if (R.Checked && movie.Certification == "R")
            {
                 result = true && result;
            }
            else
            if (Approved.Checked && movie.Certification == "Approved")
            {
                result = true && result;
            }
            else
            if (NC17.Checked && movie.Certification == "NC-17")
            {
                result = true && result;
            }
            else
            if (GP.Checked && movie.Certification == "GP")
            {
               result = true && result;
            }
            else
            if (Unrated.Checked && movie.Certification == "Unrated")
            {
                result = true && result;
            }
            else
            if (Passed.Checked && movie.Certification == "Passed")
            {
                result = true && result;
            }
            else
            if (Not.Checked && movie.Certification == "Not")
            {
                result = true && result;
            }
            else
            if (X.Checked && movie.Certification == "X")
            {
               result = true && result;
            }
            else
            if (TVG.Checked && movie.Certification == "TV-G")
            {
               result = true && result;
            }
            else
            if (TVMA.Checked && movie.Certification == "TV-MA")
            {
               result = true && result;
            }
            else
            if (TV14.Checked && movie.Certification == "TV-14")
            {
                 result = true && result;
            }
            else
                if (NLA.Checked && movie.Certification == null)
                {
                        result = true && result;
                }
                else
                {
                    result = false;
                }
            //check Time
            if (TimeGuage.Text == "<=" && movie.Itime <= range)
            {
                result = true && result;
            }
            else
                if (TimeGuage.Text == ">=" && movie.Itime >= range)
                {
                        result = true && result;
                }
                else
                    if (movie.Itime == range)
                    {
                        result = true && result;
                    }
                    else
                    {
                        result = false;
                    }
            //check Title
            if (!allTitle)
            {
                if (titleIndex == 0 && Convert.ToChar(movie.Title.Substring(0, 1)) < 'A')
                {
                    result = true && result;
                }
                else
                    if (Convert.ToChar(movie.Title.Substring(0, 1)) == AtoZ[titleIndex])
                    {
                        result = true && result;
                    }
                    else
                    {
                        result = false;
                    }
            }

            //check Director
            if (!allDirector)
            {
                if (Convert.ToChar(movie.Director.Substring(0, 1)) == AtoZ[directorIndex])
                {
                    result = true && result;
                }
                else
                {
                    result = false;
                }
            }
            //Actors
            if (!allActor)
            {
                int x = 0;
                Boolean yes = false;
                while (x < movie.Actor.Length && !yes)
                {
                    if (Convert.ToChar(movie.Actor[x].Substring(0, 1)) == AtoZ[actorIndex])
                    {                  
                        yes = true;
                    }
                    x++;
                }
                result = yes && result;
            }
            if (comboBox8.Text == "All")
            {
                result = true && result;
            }
            else
            {
                int y = 0;
                Boolean yes = false;
                while (y < movie.Genre.Length && !yes)
                {
                    String value = comboBox8.Text;
                    if (value == movie.Genre[y])
                    {
                        yes = true;
                    }
                    y++;
                }
                if (yes)
                    result = yes && result;
                else
                    result = false;
            }
            return result;
        }

        private void movieGraph_MouseClick(object sender, MouseEventArgs e)
        {
             Point loc = e.Location;
             Point temp;

            int height = (movieGraph.Height * 9)/10;
            int width = (movieGraph.Width * 8)/10;

            Pen pen = new Pen(Color.DarkCyan, 1);

            int upY = height/20;
            int downY = height;
            int frontX = width/20;
            int backX = width;

            //scale
            int scaleX = backX / 23;
            int scaleY = downY / 10;
            int x = 0;
            Boolean found = false;
            int radius = scaleY / 9;


            while (x < Movies.Count && !found)
            {
                Movie movie = Movies[x];
                temp = movie.graphPoint;
                if (loc.X + radius >= temp.X && loc.X - radius < temp.X + (scaleY / 10)
                    && loc.Y + radius >= temp.Y && loc.Y - radius < temp.Y + (scaleY / 14))
                {
                    found = true;
                    MapPreview.Location = new Point((temp.X + 10), (MapPreview.Height + temp.Y > movieGraph.Height)?
                        temp.Y - MapPreview.Height :(temp.Y + 10));
                    showMovie(movie);
                    MapPreview.Visible = true;
                }
                x++;
            }
            
            if (!found)
            {
                MapPreview.Visible = false;
            }
        }
        private void showMovie(Movie movie)
        {
            
            Graphics g = MapPreview.CreateGraphics();
            g.Clear(Color.WhiteSmoke);


            using (Font myFont = new Font("Arial", 11, FontStyle.Bold))
            { g.DrawString(TruncateAtWord(movie.Title, MapPreview.Width - 30), myFont, Brushes.Black, new PointF(10, 10)); }

            using (Font myFont = new Font("Arial", 8))
            {
                g.DrawString("Director: " + movie.Director, myFont, Brushes.Black, new PointF(10, 30));
                g.DrawString("Year: " + movie.Year, myFont, Brushes.Black, new PointF(10, 45));

                if (movie.Certification != null)
                {
                    int size = (movie.Certification.Length == 1) ? 14 : (movie.Certification.Length == 2) ?
                        22 : (movie.Certification.Length * 8);
                    g.DrawRectangle(Pens.Red, 110, 45, size, 15);
                    g.DrawString(movie.Certification, myFont, Brushes.Black, new PointF(110, 45));
                }

                g.DrawString("Rating: " + movie.Rating, myFont, Brushes.Black, new PointF(10, 60));
                g.DrawString("Length: " + movie.Time, myFont, Brushes.Black, new PointF(10, 75));

                int i = 90;
                g.DrawString("Genre: ", myFont, Brushes.Black, new PointF(10, i));
                foreach (String str in movie.Genre)
                {
                    g.DrawString(str, myFont, Brushes.Black, new PointF(10, i += 15));
                }

                i = 90;
                g.DrawString("Actor: ", myFont, Brushes.Black, new PointF(110, i));

                foreach (String str in movie.Actor)
                {
                    g.DrawString(str, myFont, Brushes.Black, new PointF(110, i += 15));
                }
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            range = trackBar1.Value;
            LenghtValue.Text = range + " mins";
            movieGraph.Refresh();
        }

        private void G_CheckedChanged(object sender, EventArgs e)
        {
            movieGraph.Refresh();
        }

        private void PG13_CheckedChanged(object sender, EventArgs e)
        {
            movieGraph.Refresh();
        }

        private void PG_CheckedChanged(object sender, EventArgs e)
        {
            movieGraph.Refresh();
        }

        private void GP_CheckedChanged(object sender, EventArgs e)
        {
            movieGraph.Refresh();
        }

        private void X_CheckedChanged(object sender, EventArgs e)
        {
            movieGraph.Refresh();
        }

        private void Not_CheckedChanged(object sender, EventArgs e)
        {
            movieGraph.Refresh();
        }

        private void TV14_CheckedChanged(object sender, EventArgs e)
        {
            movieGraph.Refresh();
        }

        private void TVMA_CheckedChanged(object sender, EventArgs e)
        {
            movieGraph.Refresh();
        }

        private void TVG_CheckedChanged(object sender, EventArgs e)
        {
            movieGraph.Refresh();
        }

        private void NC17_CheckedChanged(object sender, EventArgs e)
        {
            movieGraph.Refresh();
        }

        private void Unrated_CheckedChanged(object sender, EventArgs e)
        {
            movieGraph.Refresh();
        }

        private void R_CheckedChanged(object sender, EventArgs e)
        {
            movieGraph.Refresh();
        }

        private void Passed_CheckedChanged(object sender, EventArgs e)
        {
            movieGraph.Refresh();
        }

        private void Approved_CheckedChanged(object sender, EventArgs e)
        {
            movieGraph.Refresh();
        }

        private void NLA_CheckedChanged(object sender, EventArgs e)
        {
            movieGraph.Refresh();
        }

        private void YearFrom_SelectedIndexChanged(object sender, EventArgs e)
        {
            movieGraph.Refresh();
        }

        private void YearTo_SelectedIndexChanged(object sender, EventArgs e)
        {
            movieGraph.Refresh();
        }

        private void RatingGuage_SelectedIndexChanged(object sender, EventArgs e)
        {
            movieGraph.Refresh();
        }

        private void RatingMeter_ValueChanged(object sender, EventArgs e)
        {
            movieGraph.Refresh();
        }

        private void TimeGuage_SelectedIndexChanged(object sender, EventArgs e)
        {
            movieGraph.Refresh();
        }

        private void selectAll_CheckedChanged(object sender, EventArgs e)
        {
            G.CheckState = selectAll.CheckState;
            PG.CheckState = selectAll.CheckState;
            PG13.CheckState = selectAll.CheckState;
            R.CheckState = selectAll.CheckState;
            Approved.CheckState =  selectAll.CheckState;
            NC17.CheckState = selectAll.CheckState;
            GP.CheckState = selectAll.CheckState;
            Unrated.CheckState = selectAll.CheckState;
            Passed.CheckState = selectAll.CheckState;
            Not.CheckState = selectAll.CheckState;
            X.CheckState = selectAll.CheckState;
            TVG.CheckState = selectAll.CheckState;
            TVMA.CheckState = selectAll.CheckState;
            TV14.CheckState = selectAll.CheckState;
            NLA.CheckState = selectAll.CheckState;
        }

        private void AllTitle_Click(object sender, EventArgs e)
        {
            allTitle = true;
            TitleFilter.Text = "All";
            movieGraph.Refresh();
        }

        private void AllDirector_Click(object sender, EventArgs e)
        {
            allDirector = true;
            DirectorFilter.Text = "All";
            movieGraph.Refresh();
        }

        private void AllActor_Click(object sender, EventArgs e)
        {
            allActor = true;
            ActorFilter.Text = "All";
            movieGraph.Refresh();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            allTitle = false;
            titleIndex = trackBar2.Value;
            TitleFilter.Text =Convert.ToString(AtoZ[titleIndex]);
            movieGraph.Refresh();
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            allDirector = false;
            directorIndex = (trackBar3.Value == 0) ? 1 : trackBar3.Value;
            DirectorFilter.Text = Convert.ToString(AtoZ[directorIndex]);
            movieGraph.Refresh();
        }

        private void trackBar5_Scroll(object sender, EventArgs e)
        {
            allActor = false;
            actorIndex = (trackBar5.Value == 0)? 1 : trackBar5.Value;
            ActorFilter.Text = Convert.ToString(AtoZ[actorIndex]);
            movieGraph.Refresh();
        }

        private void mapTool_Click(object sender, EventArgs e)
        {
            MapPreview.Visible = false;
        }

        private void Clear_Click(object sender, EventArgs e)
        {
          
            allTitle = true;
            TitleFilter.Text = "All";
            trackBar2.Value = 0;

            allDirector = true;
            DirectorFilter.Text = "All";
            trackBar3.Value = 0;

            allActor = true;
            ActorFilter.Text = "All";
            trackBar5.Value = 0;

            YearFrom.Text = "1900";
            YearTo.Text = "2015";

            RatingGuage.Text = "<=";
            RatingMeter.Value = 10;

            TimeGuage.Text = "<=";
            trackBar1.Value = 350;
            LenghtValue.Text = 350 + "mins";

            comboBox8.Text = "All";
            selectAll.Checked = true;
            movieGraph.Refresh();
        }

        private void comboBox8_SelectedIndexChanged(object sender, EventArgs e)
        {
            movieGraph.Refresh();
        }

        private void FeaturedlistView_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void FeaturedlistView_DoubleClick(object sender, EventArgs e)
        {
            int i = 0;
            Boolean done = false;

            foreach (ListViewItem item in FeaturedlistView.SelectedItems)
            {
                //Console.WriteLine(item.Text);
                while (i < Movies.Count && !done)
                {   
                    if (Movies[i].Title.Equals(item.Text))
                    {
                        preview(Movies[i]);
                        done = true;
                    }
                    i++;
                }
            }
        }
    }
    public class HashFrequency
    {
        private String [,]table  = new String[1000,2];
        public int Count = 0;
     
        public void Add(String item)
        {
            int i = 0;
            Boolean done = false;
            int value = 0;
            if (Count < table.Length)
            {
                while (i < Count && !done)
                {
                    if (table[i, 0] == item)
                    {
                        value = Int32.Parse(table[i, 1]) + 1;
                        table[i, 1] = value + "";
                        done = true;
                    }
                    i++;
                }
                if (!done && Count < table.Length)
                {
                    table[Count, 0] = item;
                    table[Count, 1] = "1";
                    Count++;
                }
            }
        }
        public String[] list
        {
            get
            {
                String[] result = new String[20];
                for (int i = 0; i < 20 && table[i, 0] != null; i++)
                {
                    result[i] = table[i, 0];
                }
                return result;
            }
        }
       
    }

    public class Movie
    {
        private String year;
        private String rating;
        private String time;
        public Boolean Like

        { get; set; }
        public String Title
        {  get; set;  }
        public String Year
        {
            get { return year != null ? year : ""; }
            set { year = value; }
        }
        public int Iyear
        {
            get { return Int32.Parse(year); }
        }
        public String[] Actor
        {  get; set; }
        public String Certification
        {  get; set; }
        public String Rating
        {
            get
            {
                int num;
                String r = "";
                if (int.TryParse(rating, out num) && 0 < num && num <= 10)
                {
                    for (int i = 1; i <= num; i++)
                    {
                        r += char.ConvertFromUtf32(9733) + "";
                    }
                }
                return r;
            }
            set { rating = value; }
        }
        public int Irate
        {
            get { return Int32.Parse(rating); }
        }
        public int Itime
        {
            get { return Int32.Parse(time.Substring(0, time.IndexOf(" "))); }
        }
        public String Time
        {
            get { return time != null ? time : ""; }
            set { time = value; }
        }

        public String[] Genre
        {   get;   set; }

        public String Director
        { get; set; }

        public Boolean watchList
        { get; set; }

        public Point graphPoint
        { get; set; }
    }
}

