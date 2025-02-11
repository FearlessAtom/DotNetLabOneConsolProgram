﻿using System.Security.Cryptography.X509Certificates;
using System;
using BankLibrary;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Security.Principal;
using System.Diagnostics.Metrics;
using System.Globalization;

public class ConsoleOutput : IOutput
{
    public void Write(string message)
    {
        Console.WriteLine(message);
    }
}
public class ConsoleInput : IInput
{
    public string Read()
    {
        
        return Console.ReadLine();
    }
}
public class Program
{
   
    delegate string[] Dmenu();//зберігає два значення , перше це номер карти а друге пінкод 
    delegate double WorkInBankOperation(string cardName, string cardCod, double  moneyBank);//делегат для працювання з терміналом
    delegate double OperationinUser(string cardName); //делегат для працювання з карткою юзера (подивитись баланс або перекинути гроші на іншу картку)
    
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        double BankBalance = 1000; // це баланс банкомата
        ConnectionManager connectionManager = new ConnectionManager("Server=RIPLOVIK\\RIPLOVIK;Database=BankDatabase;Trusted_Connection=True;");;
        Bank menu = new Bank(connectionManager);
        Dmenu MenuChuseBank = menu.MenuBank;
        string[] cardDetails = MenuChuseBank();
        string cardName = cardDetails[0];
        string cardCod = cardDetails[1];
        MenuChoisInBabkOperation(cardName, cardCod, connectionManager,BankBalance);
       
        
    }
    static void MenuChoisInBabkOperation(string cardName,string cardCod, ConnectionManager connectionManager, double BankBalance)
    {
        Account userOperation = new Account(connectionManager);
        AutomatedTellerMachine ATM = new AutomatedTellerMachine(connectionManager); // делегат для працювання з терміналом


        WorkInBankOperation withdrawalOperation = ATM.withdrawalOfFundsInBank;
        WorkInBankOperation addMoneyOperation = ATM.AddMoneyInYorCard;
 

        while (true)
        {
            Console.WriteLine("Аутентифікація успішна. оберіть нступні дії \n 1-переглянути баланс\n 2-зняття коштів \n 3-зарахування коштів на картку \n 4- перерахування коштів на картку із заданим номером.\n 5 - вихід");
            string input = Console.ReadLine();
            int choix;
            OperationinUser userBalance;
            userBalance = userOperation.ChekBalans;

            if (int.TryParse(input, out choix))
            {
                switch (choix)
                {
                    case 1:
                        Console.WriteLine("Перегляд балансу.");                                      
                        Console.WriteLine(userBalance(cardName));
                        break;

                    case 2:
                        Console.WriteLine("Зняття коштів.");
                        BankBalance = withdrawalOperation(cardName, cardCod, BankBalance);                                                    
                        break;

                    case 3:
                        Console.WriteLine("Зарахування коштів на картку.");
                        BankBalance = addMoneyOperation(cardName, cardCod, BankBalance);
                        break;

                    case 4:
                        Console.WriteLine("Перерахування коштів на картку. Введіть номер картки:");
                        string cardAnotherCard = Console.ReadLine();

                        Console.WriteLine("Введіть пін-код:");
                        string pinCodeInput = Console.ReadLine();                    
                        if (cardAnotherCard.All(char.IsDigit) && pinCodeInput.All(char.IsDigit))
                        {
                            userBalance += (string card) => userOperation.TransferringFundsToAnotherCard(card, cardAnotherCard, pinCodeInput);
                            Console.WriteLine(userBalance(cardName));
                        }
                        else
                        {
                            Console.WriteLine("Некоректний номер картки або пін-код.");
                        }
                        break;

                    case 5:
                        Console.WriteLine("Вихід з програми.");
                        Environment.Exit(0);
                        break;

                    default:
                        Console.WriteLine("Неправильний вибір. Спробуйте ще раз.");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Неправильний ввід. Будь ласка, введіть число.");
            }
        }
    }
 }
