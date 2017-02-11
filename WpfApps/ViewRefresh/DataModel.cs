namespace ViewRefresh
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Windows.Data;
    using Oxygenize;

    public class DataModel
    {
        public Guid Id { get; set; }

        public string Text { get; set; }

        public int Number { get; set; }
    }

    public abstract class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }

        protected void RaisePropertyChanged(string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName ?? String.Empty));
        }
    }

    public class DataViewModel : ViewModel
    {
        private DataModel data;

        public string Text => this.data.Text;

        public int Number => this.data.Number;

        internal DataModel Data
        {
            get
            {
                return this.data;
            }
            set
            {
                this.data = value;
                RaisePropertyChanged();
            }
        }
    }

    public class MainViewModel : ViewModel
    {
        private const int MaxItemsCount = 1000;
        private static readonly Random random = new Random(Guid.NewGuid().GetHashCode());
        private readonly ObservableCollection<DataViewModel> itemsSource;
        private string textFilter;

        static MainViewModel()
        {
            Oxygenize.Configure<DataModel>(configurator =>
            {
                configurator.WithStrategy(GenerationStrategy.Mixed);
                configurator.WithMaxStringLength(30);
                configurator.WithMinStringLength(10);
                configurator.WithValues(item =>
                {
                    item.Text = random.GenerateSurname();
                    return item;
                });
            });
        }

        public MainViewModel()
        {
            this.itemsSource = new ObservableCollection<DataViewModel>();
            this.ItemsView = new ListCollectionView(this.itemsSource)
            {
                SortDescriptions =
                {
                    new SortDescription (nameof(DataViewModel.Text), ListSortDirection.Ascending)
                }
            };
        }

        public ICollectionView ItemsView { get; }

        public string TextFilter
        {
            get
            {
                return this.textFilter;
            }
            set
            {
                this.textFilter = value;
                RaisePropertyChanged(nameof(this.TextFilter));
            }
        }

        public void ApplyFilter()
        {
            if (String.IsNullOrWhiteSpace(this.TextFilter))
            {
                this.ItemsView.Filter = null;
            }
            else
            {
                this.ItemsView.Filter = obj => (obj as DataViewModel)?.Text?.Contains(this.TextFilter) ?? false;
            }
        }

        public void ChangeItems()
        {
            if (this.itemsSource.Any())
            {
                var halfCount = this.itemsSource.Count / 2;
                var notUpdatedItems = this.itemsSource
                    .Skip(random.Next(halfCount))
                    .Take(random.Next(halfCount))
                    .Select(vm => vm.Data)
                    .ToArray();
                var newItems = notUpdatedItems
                    .Concat(Oxygenize.GenerateCases<DataModel>(random.Next(MaxItemsCount - notUpdatedItems.Length)))
                    .Shuffle(random)
                    .ToArray();

                RefreshItems(newItems);
            }
            else
            {
                RefreshItems(Oxygenize.GenerateCases<DataModel>(MaxItemsCount));
            }
        }

        public void RemoveItems()
        {
            this.itemsSource.Clear();
        }

        private void RefreshItems(IEnumerable<DataModel> items)
        {
            var itemUpdates = from dataModel in items
                              join viewModel in this.itemsSource on dataModel.Id equals viewModel.Data.Id into viewModels
                              from viewModel in viewModels.DefaultIfEmpty()
                              select new { dataModel, viewModel };

            var newItems = new List<DataViewModel>();
            foreach (var itemUpdate in itemUpdates)
            {
                if (itemUpdate.viewModel == null)
                {
                    var newViewModel = new DataViewModel { Data = itemUpdate.dataModel };
                    this.itemsSource.Add(newViewModel);
                    newItems.Add(newViewModel);
                }
                else
                {
                    itemUpdate.viewModel.Data = itemUpdate.dataModel;
                    RefreshItem(itemUpdate.viewModel);
                    newItems.Add(itemUpdate.viewModel);
                }
            }

            foreach (var obsoleteItem in this.itemsSource.ToArray().Except(newItems))
            {
                this.itemsSource.Remove(obsoleteItem);
            }
        }

        private void RefreshItem(object item)
        {
            var editableCollectionView = this.ItemsView as IEditableCollectionView;
            if (editableCollectionView != null)
            {
                editableCollectionView.EditItem(item);
                editableCollectionView.CommitEdit();
            }
        }
    }

    internal static class RandomExtensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random random)
        {
            var elements = source.ToArray();
            for (var i = elements.Length - 1; i >= 0; i--)
            {
                var swapIndex = random.Next(i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
            }
        }

        public static string GenerateSurname(this Random random)
        {
            string name = string.Empty;
            string[] currentConsonant;
            var vowels = "a,a,a,a,a,e,e,e,e,e,e,e,e,e,e,e,i,i,i,o,o,o,u,y,ee,ee,ea,ea,ey,eau,eigh,oa,oo,ou,ough,ay".Split(',');
            var commonConsonants = "s,s,s,s,t,t,t,t,t,n,n,r,l,d,sm,sl,sh,sh,th,th,th".Split(',');
            var averageConsonants = "sh,sh,st,st,b,c,f,g,h,k,l,m,p,p,ph,wh".Split(',');
            var middleConsonants = "x,ss,ss,ch,ch,ck,ck,dd,kn,rt,gh,mm,nd,nd,nn,pp,ps,tt,ff,rr,rk,mp,ll".Split(','); //Can't start
            var rareConsonants = "j,j,j,v,v,w,w,w,z,qu,qu".Split(',');
            var lengthArray = new[] { 2, 2, 2, 2, 2, 2, 3, 3, 3, 4 }; //Favor shorter names but allow longer ones
            var length = lengthArray[random.Next(lengthArray.Length)];
            for (var i = 0; i < length; i++)
            {
                var letterType = random.Next(1000);
                if (letterType < 775) currentConsonant = commonConsonants;
                else if (letterType < 875 && i > 0) currentConsonant = middleConsonants;
                else if (letterType < 985) currentConsonant = averageConsonants;
                else currentConsonant = rareConsonants;
                name += currentConsonant[random.Next(currentConsonant.Length)];
                name += vowels[random.Next(vowels.Length)];
                if (name.Length > 4 && random.Next(1000) < 800) break; //Getting long, must roll to save
                if (name.Length > 6 && random.Next(1000) < 950) break; //Really long, roll again to save
                if (name.Length > 7) break; //Probably ridiculous, stop building and add ending
            }
            var endingType = random.Next(1000);
            if (name.Length > 6)
                endingType -= (name.Length * 25); //Don't add long endings if already long
            else
                endingType += (name.Length * 10); //Favor long endings if short
            if (endingType < 400) { } // Ends with vowel
            else if (endingType < 775) name += commonConsonants[random.Next(commonConsonants.Length)];
            else if (endingType < 825) name += averageConsonants[random.Next(averageConsonants.Length)];
            else if (endingType < 840) name += "ski";
            else if (endingType < 860) name += "son";
            else if (Regex.IsMatch(name, "(.+)(ay|e|ee|ea|oo)$") || name.Length < 5)
            {
                name = "Mc" + name.Substring(0, 1).ToUpper() + name.Substring(1);
                return name;
            }
            else name += "ez";
            name = name.Substring(0, 1).ToUpper() + name.Substring(1); //Capitalize first letter
            return name;
        }
    }
}