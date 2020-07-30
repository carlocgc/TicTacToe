﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TicTacToe.Data;
using TicTacToe.FrontEnd;
using TicTacToe.Interfaces;
using TicTacToe.MessageHandlers;
using TicTacToe.Models;
using TicTacToe.Services;
using static TicTacToe.Data.StaticGameData;

namespace TicTacToe
{
    public class Game
    {
        /// <summary> Message handler functions  </summary>
        private readonly Dictionary<Command, IMessageHandler> _MessageHandlers = new Dictionary<Command, IMessageHandler>();
        /// <summary> Sends and receives messages as packets </summary>
        private IMessageService _MessageService;
        /// <summary> How many games the host/client have won </summary>
        private GameProgressData _GameProgressData;

        private PlayerTurnData _PlayerTurnData;
        /// <summary> Draws the game screen </summary>
        private ScreenDrawer _ScreenDrawer;
        /// <summary> Whether the game is running </summary>
        private Boolean _Running;
        /// <summary> Whether the game is initialised </summary>
        private Boolean _Initialised;
        /// <summary> Whether its the players turn </summary>
        private Boolean _Moving;

        /// <summary> Sets up the message handlers, called once at game start </summary>
        private void Initialise()
        {
            _GameProgressData = new GameProgressData();
            _PlayerTurnData = new PlayerTurnData();
            _MessageService = new MessageService();
            _ScreenDrawer = new ScreenDrawer(_MessageService);

            _MessageHandlers.Add(Command.GAME_DRAW, new MessageHandler(_GameProgressData, _ScreenDrawer));
            _MessageHandlers.Add(Command.MOVE_CONFIRM, new MoveConfirmHandler(_PlayerTurnData));
            _MessageHandlers.Add(Command.MOVE_DENY, new MoveDenyHandler());
            _MessageHandlers.Add(Command.BOARD_STATE, new BoardStateHandler(_GameProgressData, _PlayerTurnData));
            _MessageHandlers.Add(Command.GAME_WON, new GameWonHandler(_MessageService, _ScreenDrawer, _GameProgressData, _PlayerTurnData));
            _MessageHandlers.Add(Command.MOVE_REQUEST, new MoveRequestHandler(_MessageService, _GameProgressData, _PlayerTurnData, _MessageHandlers[Command.GAME_WON]));
            _MessageHandlers.Add(Command.EXIT, new ExitHandler());

            SetUpConnection();

            _Initialised = true;
        }

        /// <summary>
        /// Passes the packet to the handler that can handle it
        /// </summary>
        /// <param name="packet"></param>
        private void HandlePacket(Packet packet)
        {
            if (Enum.TryParse(packet.Command, true, out Command command))
            {
                _MessageHandlers[command].HandleMessage(packet.Message);
            }
        }

        /// <summary> Main game loop </summary>
        public void Run()
        {
            if (!_Initialised)
            {
                Initialise();
            }

            _Moving = _MessageService.IsHost;
            _GameProgressData.PlayerSymbol = _MessageService.IsHost ? MASTER_CHAR : SLAVE_CHAR;

            _Running = true;

            while (_Running)
            {
                _ScreenDrawer.Draw(_GameProgressData);

                if (_MessageService.IsHost)
                {
                    if (_Moving)
                    {
                        MoveModel move = GetMove();

                        _GameProgressData.GameBoard[move.X, move.Y] = MASTER_CHAR;

                        if (_GameProgressData.IsGameWon(MASTER_CHAR))
                        {
                            _MessageHandlers[Command.GAME_WON].HandleMessage(MASTER_CHAR.ToString());
                        }
                        else if (_GameProgressData.IsGameDrawn())
                        {
                            _MessageHandlers[Command.GAME_DRAW].HandleMessage("");
                        }
                        else
                        {
                            _MessageService.SendPacket(_GameProgressData.GameBoardAsPacket());
                            _Moving = false;
                        }
                    }
                    else
                    {
                        _PlayerTurnData.WaitingForClient = true;

                        Console.WriteLine("Opponent is thinking....");

                        while (_PlayerTurnData.WaitingForClient)
                        {
                            Packet moveRequest = _MessageService.AwaitPacket();
                            HandlePacket(moveRequest);
                        }

                        _Moving = true;
                    }
                }
                else
                {
                    if (_Moving)
                    {
                        _PlayerTurnData.WaitingForHost = true;

                        while (_PlayerTurnData.WaitingForHost)
                        {
                            _ScreenDrawer.Draw(_GameProgressData);
                            MoveModel move = GetMove();
                            _MessageService.SendPacket(new Packet(Command.MOVE_REQUEST.ToString(), move.ToString()));
                            Packet resp = _MessageService.AwaitPacket();
                            HandlePacket(resp);
                        }

                        _MessageService.SendPacket(new Packet(Command.PACKET_RECEIVED.ToString()));
                        _Moving = false;
                    }
                    else
                    {
                        Console.WriteLine($"Opponent is thinking...");

                        Packet packet = _MessageService.AwaitPacket();
                        HandlePacket(packet);

                        _Moving = true;
                    }
                }

                _Running = !_GameProgressData.GameWon;
            }

            // Game is complete, ask for rematch
            if (_MessageService.IsHost)
            {
                String rematch = String.Empty;

                while (rematch.ToLower() != "y" && rematch.ToLower() != "n")
                {
                    Console.WriteLine($"Rematch?... y/n");
                    rematch = Console.ReadLine() ?? "";
                }

                if (rematch == "y")
                {
                    _MessageService.SendPacket(new Packet(Command.MESSAGE.ToString(), rematch));
                    Packet resp = _MessageService.AwaitPacket();

                    if (resp.Message == "rematch")
                    {
                        _GameProgressData.ResetGame();
                        Run();
                    }
                }
            }
            else
            {
                Console.WriteLine($"Awaiting rematch instruction from host...");
                Packet packet = _MessageService.AwaitPacket();
                if (packet.Message == "y")
                {
                    _MessageService.SendPacket(new Packet(Command.MESSAGE.ToString(), "rematch"));
                    _GameProgressData.ResetGame();
                    Run();
                }
                else
                {
                    Console.WriteLine($"Host declined a rematch...");
                }
            }

            // No rematch, game will end
            Console.WriteLine($"Game Exiting...");
            Console.ReadKey();
        }

        /// <summary>
        /// Configures the network connection between two players
        /// </summary>
        private void SetUpConnection()
        {
            if (_MessageService.Connected) return;

            _MessageService.Initialise();

            if (_MessageService.IsHost)
            {
                _MessageService.WaitForClient();
            }
            else
            {
                IPAddress ip = null;

                Boolean addressValid = false;

                while (!addressValid)
                {
                    Console.WriteLine($"Enter host ip address...");
                    addressValid = IPAddress.TryParse(Console.ReadLine() ?? "", out ip);
                }

                if (ip == null)
                {
                    // TODO Handle no ip defined
                    Console.WriteLine("IP was not defined, exiting...");
                    Console.ReadKey();
                    Environment.Exit(0);
                }

                _MessageService.ConnectToHost(ip, GAME_PORT);
            }
        }

        /// <summary>
        /// Prompts for a valid move on the game board
        /// </summary>
        /// <returns></returns>
        private MoveModel GetMove()
        {
            Boolean valid = false;

            Int32 x = 0;
            Int32 y = 0;

            String lastError = String.Empty;

            while (!valid)
            {
                Console.WriteLine();
                Console.WriteLine($"Enter a valid move in format \"X,X\" (0-2)");
                if (lastError != String.Empty)
                {
                    Console.WriteLine($"{lastError}");
                }

                String input = Console.ReadLine() ?? "";

                if (input.Length > 3)
                {
                    Console.WriteLine($"Too many characters, try again...");
                    continue;
                }

                if (input.Length < 3)
                {
                    Console.WriteLine($"Too few characters, try again...");
                    continue;
                }

                if (!input.Contains(","))
                {
                    Console.WriteLine($"Missing comma, try again...");
                    continue;
                }

                String[] parts = input.Split(',');

                if (!Int32.TryParse(parts[0], out Int32 tempX))
                {
                    Console.WriteLine($"{tempX} is not a number, try again...");
                    continue;
                }

                if (tempX < 0 || tempX > 2)
                {
                    Console.WriteLine($"{tempX} is out of bounds, must be between 0-2, try again...");
                    continue;
                }

                if (!Int32.TryParse(parts[1], out Int32 tempY))
                {
                    Console.WriteLine($"{tempY} is not a number, try again...");
                    continue;
                }

                if (tempY < 0 || tempY > 2)
                {
                    Console.WriteLine($"{tempY} is out of bounds, must be between 0-2, try again...");
                    continue;
                }

                if (!_GameProgressData.IsMoveValid(new MoveModel(tempX, tempY)))
                {
                    Console.WriteLine($"({tempX}, {tempY}) is already taken by \"{_GameProgressData.GameBoard[tempX, tempY]}\", try again...");
                    continue;
                }

                x = tempX;
                y = tempY;
                valid = true;
            }
            return new MoveModel(x, y);
        }
    }
}
