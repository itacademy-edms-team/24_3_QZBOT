namespace TechnologiesAPI
{
    public class DataRepository
    {
        private readonly Dictionary<string, Dictionary<string, Question>> _data;

        public DataRepository()
        {
            _data = new Dictionary<string, Dictionary<string, Question>>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    "python",
                    new Dictionary<string, Question>(StringComparer.OrdinalIgnoreCase)
                    {
                        {
                            "variables",
                            new Question()
                            {
                                Text = "Как объявить переменную в Python?",
                                Answers = new List<string> { "x = 10", "var x = 10", "let x = 10" },
                                CorrectAnswer = "x = 10"
                            }
                        },
                        {
                            "functions",
                            new Question()
                            {
                                Text = "Как объявить функцию в Python?",
                                Answers = new List<string> { "function", "def", "lambda" },
                                CorrectAnswer = "def"
                            }
                        },
                        {
                            "list",
                            new Question()
                            {
                                Text = "Как создать пустой список в Python?",
                                Answers = new List<string> { "[]", "{}", "()" },
                                CorrectAnswer = "[]"
                            }
                        },
                        {
                            "anonfunc",
                            new Question()
                            {
                                Text = "Как объявить анонимную функцию в Python?",
                                Answers = new List<string> { "lambda", "def", "anon" },
                                CorrectAnswer = "lambda"
                            }
                        },
                        {
                            "intorcomp",
                            new Question()
                            {
                                Text = "Python - интерпретируемый или компилируемый язык? (int/comp)",
                                Answers = new List<string> { "int", "comp" },
                                CorrectAnswer = "int"
                            }
                        }
                    }
                },
                {
                    "java",
                    new Dictionary<string, Question>(StringComparer.OrdinalIgnoreCase)
                    {
                        {
                            "inheritance",
                            new Question()
                            {
                                Text = "Какой оператор используется для наследования?",
                                Answers = new List<string> { "extends", "implements", "inherits" },
                                CorrectAnswer = "extends"
                            }
                        },
                        {
                            "variables",
                            new Question()
                            {
                                Text = "Как объявить переменную в Java?",
                                Answers = new List<string> { "int x = 10;", "var x = 10;", "let x = 10;" },
                                CorrectAnswer = "int x = 10;"
                            }
                        },
                        {
                            "startprogram",
                            new Question()
                            {
                                Text = "Как называется функция, с которой начинается запуск программы?",
                                Answers = new List<string> { "const", "main", "nachat" },
                                CorrectAnswer = "main"
                            }
                        },
                        {
                            "arrays",
                            new Question()
                            {
                                Text = "Как объявить массив целых чисел в Java?",
                                Answers = new List<string> { "array<int>", "int[]", "List<Integer>" },
                                CorrectAnswer = "int[]"
                            }
                        },
                        {
                            "objectclass",
                            new Question()
                            {
                                Text = "Как создать объект класса в Java?",
                                Answers = new List<string> { "new", "create", "instance" },
                                CorrectAnswer = "new"
                            }
                        },
                    }
                }
            };
        }

        public Dictionary<string, Question> GetQuestions(string technologyName)
        {
            if (!_data.ContainsKey(technologyName))
            {
                return null;
            }
            return _data[technologyName];
        }

        public Question GetQuestion(string technologyName, string questionSlug)
        {
            if (!_data.ContainsKey(technologyName) || !_data[technologyName].ContainsKey(questionSlug))
            {
                return null;
            }
            return _data[technologyName][questionSlug];
        }
    }
}
