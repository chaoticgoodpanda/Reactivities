import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { makeAutoObservable, runInAction } from "mobx";
import {ChatComment} from "../models/Comment";
import {store} from "./Store";


//Signal R CommentStore, requires installation of SignalR
export default class CommentStore {
    comments: ChatComment[] = [];
    hubConnection: HubConnection | null = null;

    constructor() {
        makeAutoObservable(this);
    }

    createHubConnection = (activityId: string) => {
        if (store.activityStore.selectedActivity) {
            this.hubConnection = new HubConnectionBuilder()
                .withUrl('http://localhost:5000/chat?activityId=' + activityId, {
                    accessTokenFactory: () => store.userStore.user?.token!
                })
                .withAutomaticReconnect()
                .configureLogging(LogLevel.Information)
                .build()
            
            this.hubConnection.start().catch(error => console.log("Error establishing the SignalR connection.", error));
            
            //this method name 'LoadComments' needs to match exactly what we named in SignalR hub
            //loads all the comments
            this.hubConnection.on('LoadComments', (comments: ChatComment[]) => {
                runInAction(() => {
                    comments.forEach(comment => {
                        //need to cast dates not as strings but as Date types for date-fns conversion in ActivityDetailedChat
                        //need to append 'Z" for Utc format conversion because for whatever reason Dotnet doesn't format correctly
                        comment.createdAt = new Date(comment.createdAt + 'Z');
                    })
                    this.comments = comments
                });
            })

                //receive a single comment -- not array since single comment
                this.hubConnection.on('ReceiveComment', (comment: ChatComment) => {
                    runInAction(() => {
                        //need to cast dates not as strings but as Date types for date-fns conversion in ActivityDetailedChat
                        //don't need 'Z' because comment is generated client side, which adds 'Z' automatically, unlike server side (.NET)
                        //unshift(comment) puts comment at the start of the array
                        comment.createdAt = new Date(comment.createdAt);
                        this.comments.unshift(comment)
                    });
                });
            
        }
    }
    
    stopHubConnection = () => {
        this.hubConnection?.stop().catch(error => console.log("Error stopping connection.", error));
    }
    
    clearComments = () => {
        //reset comments back to empty array
        this.comments = [];
        this.stopHubConnection();
    }
    
    //sending a comment
    addComment = async (values: any) => {
        values.activityId = store.activityStore.selectedActivity?.id;
        try {
            //'SendComment' needs to be exact name of method in ChatHub
            await this.hubConnection?.invoke('SendComment', values);
        } catch (error) {
            console.log(error);
        }
    } 
}
