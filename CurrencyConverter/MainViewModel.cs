using System.Text.Json;
using System.Windows.Input;

namespace CurrencyConverter
{
    public class MainViewModel : BindableObject
    {
        private string[] _valutes;
        public string[] Valutes
        {
            get => _valutes;
            set
            {
                _valutes = value;
                OnPropertyChanged(nameof(Valutes));
            }
        }

        private string _currentDate;
        public string CurrentDate
        {
            get => _currentDate;
            set
            {
                _currentDate = value;
                OnPropertyChanged(nameof(CurrentDate));
            }
        }

        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set
            {
                _date = value;

                if (_date.DayOfYear != Now.DayOfYear)
                {
                    var m = _date.Month < 10 ? $"0{_date.Month}" : $"{_date.Month}";
                    var d = _date.Day < 10 ? $"0{_date.Day}" : $"{_date.Day}";
                    var uri = new Uri($"https://www.cbr-xml-daily.ru/archive/{_date.Year}/{m}/{d}/daily_json.js");
                    var client = new HttpClient();
                    var response = client.GetAsync(uri).Result;
                    while (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        _date = _date.AddDays(-1);
                        m = _date.Month < 10 ? $"0{_date.Month}" : $"{_date.Month}";
                        d = _date.Day < 10 ? $"0{_date.Day}" : $"{_date.Day}";
                        uri = new Uri($"https://www.cbr-xml-daily.ru/archive/{_date.Year}/{m}/{d}/daily_json.js");
                        response = client.GetAsync(uri).Result;
                    }
                    var result = response.Content.ReadAsStringAsync().Result;
                    Root = JsonSerializer.Deserialize<Root>(result);
                }
                else
                {
                    var uri = new Uri("https://www.cbr-xml-daily.ru/daily_json.js");
                    var client = new HttpClient();
                    var response = client.GetAsync(uri).Result;
                    var result = response.Content.ReadAsStringAsync().Result;
                    Root = JsonSerializer.Deserialize<Root>(result);
                }
                var month = _date.Month < 10 ? $"0{_date.Month}" : $"{_date.Month}";
                var day = _date.Day < 10 ? $"0{_date.Day}" : $"{_date.Day}";
                CurrentDate = $"{day}.{month}.{_date.Year}";
                _date = value;
                OnPropertyChanged(nameof(Date));
                if (Flag) OnPropertyChanged(nameof(ValueTo));
                if (!Flag) OnPropertyChanged(nameof(ValueFrom));
            }
        }


        private Root _root;
        public Root Root
        {
            get => _root;
            set
            {
                _root = value;
                OnPropertyChanged(nameof(Root));
            }
        }

        private string _Item1;
        public string Item1
        {
            get => _Item1;
            set
            {
                _Item1 = value;
                OnPropertyChanged(nameof(Item1));
                OnPropertyChanged(nameof(ValueFrom));
            }
        }
        private string _Item2;
        public string Item2
        {
            get => _Item2;
            set
            {
                _Item2 = value;
                OnPropertyChanged(nameof(Item2));
                OnPropertyChanged(nameof(ValueTo));
            }
        }


        private bool _flag;
        public bool Flag
        {
            get => _flag;
            set
            {
                _flag = value;
                OnPropertyChanged(nameof(Flag));
            }
        }

        private double _valueFrom;
        public string ValueFrom
        {
            get
            {
                double value1, value2, nominal1, nominal2;

                if (Item1 == "RUB") { value1 = 1; nominal1 = 1;}
                else
                {
                    value1 = Root.Valute[Item1].Value;
                    nominal1 = Root.Valute[Item1].Nominal;
                }
                if (Item2 == "RUB") { value2 = 1; nominal2 = 1;}
                else
                {
                    value2 = Root.Valute[Item2].Value;
                    nominal2 = Root.Valute[Item2].Nominal;
                }
                return Math.Round(_valueTo / (value1 / value2) * (nominal1 / nominal2), 4).ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value)) { return; }
                _valueFrom = double.Parse(value);
                if (Flag) OnPropertyChanged(nameof(ValueTo));
            }
        }

        private double _valueTo;
        public string ValueTo
        {
            get
            {
                double value1, value2, nominal1, nominal2;

                if (Item1 == "RUB") {value1 = 1; nominal1 = 1;}
                else
                {
                    value1 = Root.Valute[Item1].Value;
                    nominal1 = Root.Valute[Item1].Nominal;
                }
                if (Item2 == "RUB") { value2 = 1; nominal2 = 1;}
                else
                {
                    value2 = Root.Valute[Item2].Value;
                    nominal2 = Root.Valute[Item2].Nominal;
                }
                return Math.Round(value1 / value2 * _valueFrom * (nominal2 / nominal1), 4).ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value)) {return;}
                _valueTo = double.Parse(value);
                if (!Flag) OnPropertyChanged(nameof(ValueFrom));
            }
        }

        public MainViewModel()
        {
            Date = Now;

            Valutes = new string[Root.Valute.Count + 1];
            Valutes = Root.Valute.Keys.ToArray();

            Valutes = Valutes.Append("RUB").ToArray();

            Item1 = "RUB";
            Item2 = "RUB";

            GetDateCurrency = new Command<DateTime>(async (date) =>
            {
                var Urlstring = new Uri(Root.PreviousURL);
                var client = new HttpClient();
                var response = await client.GetAsync(Urlstring);
                var result = await response.Content.ReadAsStringAsync();

                Root = JsonSerializer.Deserialize<Root>(result);

            });

            ChangeFlag = new Command<string>((e) =>
            {
                if (e == "1") Flag = true;
                if (e == "2") Flag = false;
            });

            var timer = new Timer(state =>
            {
                OnPropertyChanged(nameof(Now));
            }, new object(), TimeSpan.Zero, TimeSpan.FromSeconds(1));


        }

        public ICommand GetDateCurrency { get; }
        public ICommand ChangeFlag { get; }

        private string _title;

        public string Title
        {
            get => _title;
            set
            {
                if (_title == value) return;
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public DateTime Now => DateTime.Now;

        private bool _someBoolProperty;
        public bool SomeBoolProperty
        {
            get => _someBoolProperty;
            set
            {
                if (_someBoolProperty == value) return;
                _someBoolProperty = value;
                OnPropertyChanged(nameof(SomeBoolProperty));
            }
        }

        private string _number;
        public string Number
        {
            get => _number;
            set
            {
                if (_number == value) return;
                _number = value;
                OnPropertyChanged(nameof(Number));
            }
        }
    }
}