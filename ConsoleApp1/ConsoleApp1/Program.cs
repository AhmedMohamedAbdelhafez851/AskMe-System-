using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/////////////////////////////// Helper Methods ///////////////////////////////
static class HelperMethods
{
    public static List<string> ReadFileLines(string path)
    {
        var lines = new List<string>();
        try
        {
            lines = File.ReadAllLines(path).ToList();
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("\n\nERROR: Can't open the file\n\n");
        }
        return lines;
    }

    public static void WriteFileLines(string path, List<string> lines, bool append = true)
    {
        var mode = append ? FileMode.Append : FileMode.Create;
        using (var fileStream = new FileStream(path, mode))
        using (var streamWriter = new StreamWriter(fileStream))
        {
            foreach (var line in lines)
            {
                streamWriter.WriteLine(line);
            }
        }
    }

    public static List<string> SplitString(string str, string delimiter = ",")
    {
        return str.Split(new string[] { delimiter }, StringSplitOptions.None).ToList();
    }

    public static int ToInt(string str)
    {
        if (int.TryParse(str, out int result))
        {
            return result;
        }
        return 0; // Default value if parsing fails
    }

    public static int ReadInt(int low, int high)
    {
        int value;
        do
        {
            Console.WriteLine($"\nEnter number in range {low} - {high}: ");
        } while (!int.TryParse(Console.ReadLine(), out value) || value < low || value > high);

        return value;
    }

    public static int ShowReadMenu(List<string> choices)
    {
        Console.WriteLine("\nMenu:\n");
        for (int ch = 0; ch < choices.Count; ++ch)
        {
            Console.WriteLine($"\t{ch + 1}: {choices[ch]}\n");
        }
        return ReadInt(1, choices.Count);
    }
}

//////////////////////////////////////////////////////////////

class Question
{
    public int QuestionId { get; set; }
    public int ParentQuestionId { get; set; }
    public int FromUserId { get; set; }
    public int ToUserId { get; set; }
    public int IsAnonymousQuestions { get; set; }
    public string QuestionText { get; set; }
    public string AnswerText { get; set; }

    public Question()
    {
        QuestionId = -1;
        ParentQuestionId = -1;
        FromUserId = -1;
        ToUserId = -1;
        IsAnonymousQuestions = 1;
        QuestionText = "";
        AnswerText = "";
    }

    public Question(string line)
    {
        var substrs = HelperMethods.SplitString(line);
        if (substrs.Count == 7)
        {
            QuestionId = HelperMethods.ToInt(substrs[0]);
            ParentQuestionId = HelperMethods.ToInt(substrs[1]);
            FromUserId = HelperMethods.ToInt(substrs[2]);
            ToUserId = HelperMethods.ToInt(substrs[3]);
            IsAnonymousQuestions = HelperMethods.ToInt(substrs[4]);
            QuestionText = substrs[5];
            AnswerText = substrs[6];
        }
    }

    public string ToString()
    {
        return $"{QuestionId},{ParentQuestionId},{FromUserId},{ToUserId},{IsAnonymousQuestions},{QuestionText},{AnswerText}";
    }

    public void PrintToQuestion()
    {
        string prefix = "";

        if (ParentQuestionId != -1)
        {
            prefix = "\tThread: ";
        }

       // Console.WriteLine($"{prefix}Question Id ({QuestionId)}");
        if (IsAnonymousQuestions != 0)
        {
            Console.WriteLine($" from user id({FromUserId})");
        }
        Console.WriteLine($"\t Question: {QuestionText}\n");

        if (!string.IsNullOrEmpty(AnswerText))
        {
            Console.WriteLine($"{prefix}\tAnswer: {AnswerText}\n");
        }
    }

    public void PrintFromQuestion()
    {
        Console.WriteLine($"Question Id ({QuestionId})");
        if (IsAnonymousQuestions != 0)
        {
            Console.WriteLine(" !AQ");
        }

        Console.WriteLine($" to user id({ToUserId})");
        Console.WriteLine($"\t Question: {QuestionText}");

        if (!string.IsNullOrEmpty(AnswerText))
        {
            Console.WriteLine($"\tAnswer: {AnswerText}\n");
        }
        else
        {
            Console.WriteLine("\tNOT Answered YET\n");
        }
    }

    public void PrintFeedQuestion()
    {
        if (ParentQuestionId != -1)
        {
            Console.WriteLine($"Thread Parent Question ID ({ParentQuestionId}) ");
        }

        Console.WriteLine($"Question Id ({QuestionId})");
        if (IsAnonymousQuestions != 0)
        {
            Console.WriteLine($" from user id({FromUserId})");
        }

        Console.WriteLine($" To user id({ToUserId})");

        Console.WriteLine($"\t Question: {QuestionText}\n");
        if (!string.IsNullOrEmpty(AnswerText))
        {
            Console.WriteLine($"\tAnswer: {AnswerText}\n");
        }
    }
}

class User
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public int AllowAnonymousQuestions { get; set; }

    public List<int> QuestionsIdFromMe { get; set; }
    public Dictionary<int, List<int>> QuestionidQuestionidsTheadToMap { get; set; }

    public User()
    {
        UserId = -1;
        AllowAnonymousQuestions = -1;
        QuestionsIdFromMe = new List<int>();
        QuestionidQuestionidsTheadToMap = new Dictionary<int, List<int>>();
    }

    public User(string line)
    {
        var substrs = HelperMethods.SplitString(line);
        if (substrs.Count == 6)
        {
            UserId = HelperMethods.ToInt(substrs[0]);
            UserName = substrs[1];
            Password = substrs[2];
            Name = substrs[3];
            Email = substrs[4];
            AllowAnonymousQuestions = HelperMethods.ToInt(substrs[5]);
        }
    }

    public string ToString()
    {
        return $"{UserId},{UserName},{Password},{Name},{Email},{AllowAnonymousQuestions}";
    }

    public void ResetToQuestions(List<int> toQuestions)
    {
        QuestionsIdFromMe.Clear();
        QuestionsIdFromMe.AddRange(toQuestions);
    }

    public void ResetFromQuestions(List<Tuple<int, int>> toQuestions)
    {
        QuestionidQuestionidsTheadToMap.Clear();
        foreach (var idPair in toQuestions)
        {
            if (!QuestionidQuestionidsTheadToMap.ContainsKey(idPair.Item1))
            {
                QuestionidQuestionidsTheadToMap[idPair.Item1] = new List<int>();
            }
            QuestionidQuestionidsTheadToMap[idPair.Item1].Add(idPair.Item2);
        }
    }

    public void ReadUser(string userName, int id)
    {
        UserName = userName;
        UserId = id;

        Console.WriteLine("Enter your password: ");
        Password = Console.ReadLine();

        Console.WriteLine("Enter your name: ");
        Name = Console.ReadLine();

        Console.WriteLine("Enter your email: ");
        Email = Console.ReadLine();

        AllowAnonymousQuestions = HelperMethods.ReadInt(0, 1);
    }

    public void PrintToUser()
    {
        Console.WriteLine($"User id ({UserId})");

        Console.WriteLine($"User name: {UserName}");
        Console.WriteLine($"Name: {Name}");
        Console.WriteLine($"Email: {Email}");

        Console.Write("Password: ");
        for (int i = 0; i < Password.Length; ++i)
        {
            Console.Write("*");
        }
        Console.WriteLine();
        Console.WriteLine($"Allow Anonymous Questions: {(AllowAnonymousQuestions != 0 ? "YES" : "NO")}");
    }

    public void PrintFromUser()
    {
        Console.WriteLine($"User id ({UserId})");

        Console.WriteLine($"User name: {UserName}");
        Console.WriteLine($"Name: {Name}");
        Console.WriteLine($"Email: {Email}");

        Console.Write("Password: ");
        for (int i = 0; i < Password.Length; ++i)
        {
            Console.Write("*");
        }
        Console.WriteLine();
        Console.WriteLine($"Allow Anonymous Questions: {(AllowAnonymousQuestions != 0 ? "YES" : "NO")}");

        Console.WriteLine($"Total Questions From you: {QuestionsIdFromMe.Count}");
        foreach (var id in QuestionsIdFromMe)
        {
            Console.WriteLine($"\tQuestion ID: {id}");
        }

        Console.WriteLine($"Threads of Questions You Participate in: {QuestionidQuestionidsTheadToMap.Count}");
        foreach (var pair in QuestionidQuestionidsTheadToMap)
        {
            Console.WriteLine($"\tThread ID: {pair.Key}");
            foreach (var id in pair.Value)
            {
                Console.WriteLine($"\t\tQuestion ID: {id}");
            }
        }
    }
}

class Program
{
    static List<User> users = new List<User>();
    static List<Question> questions = new List<Question>();

    static void LoadUsers()
    {
        var userLines = HelperMethods.ReadFileLines("Users.txt");
        foreach (var line in userLines)
        {
            users.Add(new User(line));
        }
    }

    static void LoadQuestions()
    {
        var questionLines = HelperMethods.ReadFileLines("Questions.txt");
        foreach (var line in questionLines)
        {
            questions.Add(new Question(line));
        }
    }

    static void WriteUsers()
    {
        var userLines = new List<string>();
        foreach (var user in users)
        {
            userLines.Add(user.ToString());
        }
        HelperMethods.WriteFileLines("Users.txt", userLines, false);
    }

    static void WriteQuestions()
    {
        var questionLines = new List<string>();
        foreach (var question in questions)
        {
            questionLines.Add(question.ToString());
        }
        HelperMethods.WriteFileLines("Questions.txt", questionLines, false);
    }

    static void AddUser()
    {
        var newUser = new User();
        newUser.ReadUser("NewUser", users.Count + 1);
        users.Add(newUser);
    }

    static void AddQuestion()
    {
        var newQuestion = new Question();
        newQuestion.QuestionId = questions.Count + 1;
        Console.WriteLine("\nEnter the question: ");
        newQuestion.QuestionText = Console.ReadLine();
        questions.Add(newQuestion);
    }

    static void Main(string[] args)
    {
        LoadUsers();
        LoadQuestions();

        bool running = true;
        while (running)
        {
            Console.WriteLine("\n1. Add User");
            Console.WriteLine("2. Add Question");
            Console.WriteLine("3. Print User");
            Console.WriteLine("4. Print Question");
            Console.WriteLine("5. Exit");
            int choice = HelperMethods.ReadInt(1, 5);

            switch (choice)
            {
                case 1:
                    AddUser();
                    WriteUsers();
                    break;
                case 2:
                    AddQuestion();
                    WriteQuestions();
                    break;
                case 3:
                    Console.WriteLine("\nEnter User Id: ");
                    int userId = HelperMethods.ReadInt(1, users.Count);
                    users[userId - 1].PrintFromUser();
                    break;
                case 4:
                    Console.WriteLine("\nEnter Question Id: ");
                    int questionId = HelperMethods.ReadInt(1, questions.Count);
                    questions[questionId - 1].PrintToQuestion();
                    break;
                case 5:
                    running = false;
                    break;
                default:
                    Console.WriteLine("\nInvalid choice. Please try again.");
                    break;
            }
        }
    }
}
