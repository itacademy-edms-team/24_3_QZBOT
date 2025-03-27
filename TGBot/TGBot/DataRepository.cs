namespace TGBot
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
                        }
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
