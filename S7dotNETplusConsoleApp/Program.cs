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
        static byte[] rcvData = { };    // data that is read from PLC memory
        static byte[] sendData = {1, 1, 1, 1, 1, 1, 1, 1 };   // data that is sent to PLC memory

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
            rcvData = plc.ReadBytes(DataType.DataBlock, 30, 14, 8);     // read data and save them to a global variable "data" 
            bRcvd = true;                                               // set a global boolean indicator to "true"
        }

        
        static void SendData(Plc plc)
        {
            PrepareData();
            ErrorCode _error = plc.WriteBytes(DataType.DataBlock, 30, 0, sendData);
            if (_error == ErrorCode.NoError)
            {
                bSent = true;
            }
        }

        static void PrepareData()
        {
            sendData[0] = rcvData[0];
            sendData[2] = (int)1;
            sendData[4] = (int)5;
            sendData[6] = (byte)'i';
            sendData[7] = (byte)'i';
            /*
            sendData[8] = (byte)'i';
            sendData[9] = (byte)'i';
            sendData[10] = (byte)'i';
            sendData[11] = (byte)'i';
            sendData[12] = (byte)'i';
            sendData[13] = (byte)'i';
        */
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
                        bRcvd = false;                                                    // reset boolean value
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
