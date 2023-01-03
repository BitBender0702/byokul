// import { Injectable } from "@angular/core";

// import * as signalR from '@microsoft/signalr'

// @Injectable({
//     providedIn: 'root'
// })

// export class SignalrService{
//     constructor(){

//     }

//     private thenable!: Promise<void>
//     hubConnection!:signalR.HubConnection;

//     startConnection = () => {
//         this.hubConnection = new signalR.HubConnectionBuilder()
//         .withUrl('/chatHub', {
//             skipNegotiation: true,
//             transport: signalR.HttpTransportType.WebSockets
//         })
//         .configureLogging(signalR.LogLevel.Information)
//         .build();

//         //this.start()
        
    
//         this.hubConnection.start()
//         .then(() => {
//             console.log('Hub Connection Started!');
//         })
//         .catch(err => console.log('Error while starting connection: ' + err))
//     }

//     askServer() {
//         this.hubConnection.invoke("SendToUser", "user","1","Hey")
//             .catch(err => console.error(err));


//             // this.thenable.then(() =>{
//             //     this.hubConnection
//             //        .invoke('askServer', "hey")
//             //        .catch(err => console.error(err));
//             // });
            
//     }
    
//     askServerListener() {
//         this.hubConnection.on("ReceiveMessage", (someText) => {
//             console.log(someText);
//         })
//     }
// }


import { Injectable } from '@angular/core';
import * as signalR from "@microsoft/signalr"
import { getuid } from 'process';
import { Subject } from 'rxjs';
import { ChartModel } from '../interfaces/chat/ChatModel';
import { CustomXhrHttpClient } from './signalr.httpclient';

export const signalRResponse =new Subject<{receiver : string , message: string, isTest : boolean}>();  


@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  public data!: ChartModel[];
  public bradcastedData!: ChartModel[];


  private hubConnection!: signalR.HubConnection
    public startConnection = () => {
      this.hubConnection = new signalR.HubConnectionBuilder()
                              .withUrl('https://localhost:7220/chatHub', 
                               {
                                            skipNegotiation: true,
                                            transport: signalR.HttpTransportType.WebSockets
                                        } )
                              .build();
      this.hubConnection
        .start()
        .then(() => console.log('Connection started'))
        .catch(err => console.log('Error while starting connection: ' + err))


   

    
  //   this.hubConnection.start().then( () => {
  //     console.log($("#userID").val());
  //     this.hubConnection.invoke("GetConnectionId", $("#userID").val()).then(function (id)
  //    {
  //         // document.getElementById("connectionId").innerText = id;
  //     });
  //     // document.getElementById("sendButton").disabled = false;
  // }).catch(function (err) {
  //     return console.error(err.toString());
  // });

  
}

askServer(userId:string) {
  this.hubConnection.invoke("GetConnectionId", userId)
  .catch(err => console.error(err));
}

sendToUser(userName:string,recieverId:string,messageToUser:string){
  this.hubConnection.invoke("SendToUser", userName,recieverId,messageToUser)
  .catch(err => console.error(err));
}

askServerListener(){
this.hubConnection.on("ReceiveMessage", (user,message) => {
  signalRResponse.next({receiver: user, message : message, isTest : true});
              console.log(`this ${user} send ${message}`);
              
          })
}

sendToGroup(userId:string,messageToUser:string){
  this.hubConnection.invoke("SendMessageToGroup", 'test',userId,messageToUser)
  .catch(err => console.error(err));
}

createGroupName() {
  var groupName = 'test';
  this.hubConnection.invoke("JoinGroup", groupName)
  .catch(err => console.error(err));
}

// document.getElementById("sendGroup").addEventListener("click", function (event) {
    
    // public addTransferChartDataListener = () => {
    //   this.hubConnection.on('transferchartdata', (data) => {
    //     this.data = data;
    //     console.log(data);
    //   });
    // }

    // public broadcastChartData = () => {
    //   const data = this.data.map(m => {
    //     const temp = {
    //       data: m.data,
    //       label: m.label
    //     }
    //     return temp;
    //   });

    //   this.hubConnection.invoke('broadcastchartdata', data)
    //   .catch(err => console.error(err));
    // }

    // public addBroadcastChartDataListener = () => {
    //   this.hubConnection.on('broadcastchartdata', (data) => {
    //     this.bradcastedData = data;
    //   })
    // }
}


