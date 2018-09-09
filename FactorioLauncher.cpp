#include <stdio.h>
#include <conio.h>
#include <windows.h>

using namespace std;

int checker(char input[],char check[])
{
	int i,result=1;
	for(i=0; input[i]!='\0' || check[i]!='\0'; i++) {
		if(input[i] != check[i]) {
			result=0;
			break;
		}
	}
	return result;
}

char* ReadLine(FILE *fp) {
    char * line = (char*)malloc(100), * linep = line;
    size_t lenmax = 100, len = lenmax;
    int c;

    if(line == NULL)
        return NULL;

    for(;;) {
        c = fgetc(fp);
        if(c == EOF)
            break;

        if(--len == 0) {
            len = lenmax;
            char * linen = (char*)realloc(linep, lenmax *= 2);

            if(linen == NULL) {
                free(linep);
                return NULL;
            }
            line = linen + (line - linep);
            linep = linen;
        }

        if((*line++ = c) == '\n')
            break;
    }
    *line = '\0';
    return linep;
}

char* getline(void) {
    char * line = (char*)malloc(100), * linep = line;
    size_t lenmax = 100, len = lenmax;
    int c;

    if(line == NULL)
        return NULL;

    for(;;) {
        c = fgetc(stdin);
        if(c == EOF)
            break;

        if(--len == 0) {
            len = lenmax;
            char * linen = (char*)realloc(linep, lenmax *= 2);

            if(linen == NULL) {
                free(linep);
                return NULL;
            }
            line = linen + (line - linep);
            linep = linen;
        }

        if((*line++ = c) == '\n')
            break;
    }
    *line = '\0';
    return linep;
}

int checkserver()
{
	system("cls");
	printf("Server checking system.\nPlease enter ip in format xxx.xxx.xxx.xxx\n Ip:");
	char ipstr[80];
	fgets(ipstr, sizeof(ipstr), stdin);
	ipstr[strlen(ipstr)-1] = 0;
	
	printf("Please enter port (default 34197):");
	char portstr[80];
	fgets(portstr, sizeof(portstr), stdin);
	int portnum = atoi(portstr);
	
	printf("Getting server info on %s:%d . Please wait...\n", ipstr, portnum);
	char *cmdstr = (char*)malloc(100000);
	sprintf(cmdstr,"getserverinfo.exe %s:%d", ipstr, portnum);
	system(cmdstr);
	
	char *filename;
	sprintf(filename, "%s_%d.sinf", ipstr, portnum);
	FILE *desc = NULL;
	desc = fopen(filename, "r");
	
	if(desc == NULL)
	{
		printf("Something went wrong...\nPlease report logs on q3.max.2011@ya.ru\n                      (roma-svistunov@tut.by)\nPress any key...");
		getch();
		return -1;
	}
	
	char *onlineofflinestr;
	onlineofflinestr = ReadLine(desc);
	printf("Server currently %s\n", onlineofflinestr);
	if(checker(onlineofflinestr, (char*)"OFFLINE"))
	{
		getch();
		return 0;
	}
	else
	{
		printf("    %s\n", ReadLine(desc));
		int Summ = 0;
		int adminsonline = 0;
		int playersonline = 0;
		adminsonline = atoi( ReadLine(desc) );
		printf("Admins online --= %d =-- :\n", adminsonline);
		for(int counter = 0; counter < adminsonline; counter++)
		{
			printf("                  %s", ReadLine(desc));
		}
		playersonline = atoi(ReadLine(desc));
		printf("Players online %d:\n", playersonline);
		for(int counter = 0; counter < playersonline; counter++)
		{
			printf("              %s", ReadLine(desc));
		}
		int modscount = atoi(ReadLine(desc));
		printf("TOTAL: %d\nMods %d:\n", adminsonline+playersonline, modscount);
		for(int counter = 0; counter<modscount; counter++)
		{
			printf("- %s", ReadLine(desc));
		}
		printf("\nPress F to pay respects\r");
		Sleep(500);
		printf("Press any key...                  \n");
		char c;
		c=getch();
	}
	return 0;
}

void PrintMenu()
{
	system("cls");
	printf("  ______            _                _                \n");
	printf(" |  ____|          | |              (_)               \n");
	printf(" | |__  __ _   ___ | |_  ___   _ __  _   ___          \n");
	printf(" |  __|/ _` | / __|| __|/ _ \\ | '__|| | / _ \\         \n");
	printf(" | |  | (_| || (__ | |_| (_) || |   | || (_) |        \n");
	printf(" |_|   \\__,_| \\___| \\__|\\___/ |_|   |_| \\___/         \n");
	printf("  _                                 _                 \n");
	printf(" | |      Console Edition          | | beta v0.0.1    \n");
	printf(" | |      __ _  _   _  _ __    ___ | |__    ___  _ __ \n");
	printf(" | |     / _` || | | || '_ \\  / __|| '_ \\  / _ \\| '__|\n");
	printf(" | |____| (_| || |_| || | | || (__ | | | ||  __/| |   \n");
	printf(" |______|\\__,_| \\__,_||_| |_| \\___||_| |_| \\___||_|   \n");
	printf("													  \n");
	printf("													  \n");
	printf("                by MaX and _romanchick_               \n");
	printf("                                                      \n");
	printf("        Press \"E\" to check server.\n");
	printf("        Changing modpacks still works so bad...\n");
	printf("        Stable opening Factorio not ready yet...\n");
	printf("        Press \"Q\" to quit.\n");
}

int main()
{
	char ch = ' ';
	bool exit = false;
	while(!exit)
	{
		PrintMenu();
		ch = getch();
		switch(ch)
		{
			case 'q':
			case 'Q':
			exit = true;
			system("cls");
			break;
			
			case 'E':
			case 'e':
			checkserver();
			break;
			
		}	
	}
	return 0;
}