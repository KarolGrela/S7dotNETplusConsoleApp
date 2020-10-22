using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S7;
using S7.Net;

namespace S7dotNETplusConsoleApp
{
    class Program
    {
        /// <summary>
        /// Global data used both by Main function and a RcvData function
        /// </summary>
        static bool bRcvd;
        static bool bSent;
        static byte[] rcvData = { 1, 2, 3, 4, 5, 6, 7, 8 };    // data that is read from PLC memory
        static byte[] sendData = {123,2,3,4,5,6,7,8};   // data that is sent to PLC memory

        /// <summary>
        /// Method called by timer while setting off
        /// </summary>
        /// <param name="plc"> instance of "Plc" variable </param>
        static void RcvData(Plc plc)
        {
            /// <summary>
            /// Method "ReadBytes" is used to read data from memory of the PLC
            /// </summary>
            /// <param name="dataType"> specifies where the read data is stored (DB, Memory, Inputs, Outputsm etc  </param>
            /// <param name="db"> address of the datatype, eg. if we reaed from DB45 then db = 45 </param>
            /// <param name="startByteAdr"> address of first byte that is to be read </param>
            /// <param name="count"> amount of read bytes </param>
            rcvData = plc.ReadBytes(DataType.DataBlock, 30, 8, 6);     // read data and save them to a global variable "data" 
            bRcvd = true;                                               // set a global boolean indicator to "true"
        }

        
        static void SendData(Plc plc)
        {
            PrepareData();      // prepare sendData array

            ErrorCode _error = plc.WriteBytes(DataType.DataBlock, 30, 0, sendData);     // send data
            if (_error == ErrorCode.NoError)                                            // no error detected
            {
                bSent = true;
            }
        }

        /// <summary>
        /// Function saving data to "sendData" array
        /// </summary>
        static void PrepareData()
        {
            // preparing boolean value
            if (sendData[0] == 123) sendData[0] = 1;    // first cycle
            else
            {
                // if bool is true, change to false and vice versa
                if (sendData[0] == 1)
                {
                    sendData[0] = 0;
                }
                else
                {
                    sendData[0] = 1;
                }
            }

            // saving integer values
            short x = 30;
            short y = 111;
            // converting integers to byte[] arrays and reversing order (S7 uses different notation)
            byte[] xB = BitConverter.GetBytes(x);
            Array.Reverse(xB);
            byte[] yB = BitConverter.GetBytes(y);
            Array.Reverse(yB);

            sendData[2] = xB[0];
            sendData[3] = xB[1];
            sendData[4] = yB[0];
            sendData[5] = yB[1];
            sendData[6] = (byte)'k';
            
        }


        static void Main(string[] args)
        {
            // user "interface"
            Console.WriteLine("Begin the program:");
            Console.WriteLine("Hit enter in order to connect with PLC");
            Console.ReadLine();

            Plc plc = new Plc(CpuType.S71500, "192.168.0.2", 0, 1);     // instance of a "Plc" variable
            ErrorCode _error = plc.Open();                              // open connection with PLC controller

            string message = "Error code message: ";                    // create message for user
            message += _error;                                          // add information about error code (NoError if connection has been established succesfully)
            Console.WriteLine(message);                                 // display the message

            
            if (_error == ErrorCode.NoError)                            // if connection has been established sccesfully
            {
                // setting timer
                System.Timers.Timer aTimer = new System.Timers.Timer(2000); // interval = 2000ms
                aTimer.Elapsed += (sender, e) => RcvData(plc);              // add event which points to a function "RcvData" 
                aTimer.Elapsed += (sender, e) => SendData(plc);
                aTimer.Enabled = true;                                      // enabled = true -> timer will rise Elapsed event
                aTimer.AutoReset = true;                                    // enables auto-resetting after finishing cycle

                while (plc.IsConnected)                                     // as long as PLC is connected this loop will 
                {
                    if (bRcvd)  // if global boolean has been set to "true", print data
                    {
                        DateTime time_st = DateTime.Now;                                  // get timestamp
                        Console.WriteLine(time_st + ": data rcvd, " + rcvData.Length);    // write message
                        
                        // saving boolean data
                        bool bit = BitConverter.ToBoolean(rcvData, 0);

                        // saving integer values
                        byte[] iData = { 1, 2 };
                        iData[0] = rcvData[3];
                        iData[1] = rcvData[2];
                        short z = BitConverter.ToInt16(iData, 0);
                        
                        // saving char values
                        char c = BitConverter.ToChar(rcvData, 4);

                        // writing values to console
                        Console.WriteLine($"{bit}, {z}, {c}");

                        // reset boolean value
                        bRcvd = false;                                                    
                    }

                    if (bSent)
                    {
                        Console.WriteLine("Data have been sent!");
                        bSent = false;
                    }
                }
            }
            // if connection hasn't been established succesfully
            else
            {
                Console.WriteLine("Connection with PLC couldn't be established. Press enter to exit the application");
            }
            
            Console.ReadLine(); // press enter to finish the program
            plc.Close();        // resolve connection with the PLC
        }


    }

}
