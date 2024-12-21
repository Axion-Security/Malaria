# Malaria - C# Windows Forms Application Ransomware
![image](https://github.com/user-attachments/assets/3dc4112f-0094-4734-9743-efdff3ad37d8)

Malaria is a Windows Forms application designed to run automatically when the user logs in, disrupt the user interface, and prevent the termination of the application. The main functionalities of the app include modifying startup behavior, displaying an image on all screens, blocking Task Manager, and overriding window procedures to prevent users from interacting with system functions like ALT+F4, ALT+TAB, and ALT+ESC. Built in .NET 4.8

## Features

- **Auto Start:** 
  - Adds the application to the Windows startup registry key to ensure it launches automatically when the user logs in.
  
- **Remote Shell:** 
  - Connects to a Remote Server like netcat. You can then run commands externally in CMD of the victim.
    
- **Display Image on All Screens:**
  - Displays image on all connected screens, setting it as the background of a borderless, maximized form.

- **Kill Task Manager:**
  - Continuously monitors and terminates the Task Manager process to prevent the user from closing the application or viewing system processes.

- **Override Window Procedures:**
  - Blocks certain key combinations (e.g., ALT+F4, ALT+TAB, and ALT+ESC) to prevent the user from switching away from or closing the application.

- **Multi-threading Support:**
  - Utilizes multi-threading to handle the auto-start and task manager killing functionalities concurrently, ensuring smooth operation without user interruption.

## Disclaimer

This project is intended for educational and research purposes only. It is essential to use such tools responsibly and ensure that they are not used maliciously or in violation of any laws. 

## How to Use

###Without Remote Shell

- Download the release and run the .exe
  
###With Remote Shell

- Download the sourcecode and change in Form1 the ip and port then build it and run the created .exe


## License

This project is open-source and available under the MIT License. See [LICENSE](LICENSE) for more information.
