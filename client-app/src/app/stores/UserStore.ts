import { makeAutoObservable, runInAction } from "mobx";
import {User, UserFormValues} from "../models/User";
import Agent from "../api/agent";
import {store} from "./Store";
import {history} from "../../index";

export default class UserStore {
    user: User | null = null;
    fbAccessToken: string | null = null;
    fbLoading = false;
    refreshTokenTimeout: any;
    
    constructor() {
        makeAutoObservable(this);
    }
    
    get isLoggedIn() {
        return!!this.user;
    }
    
    login = async (creds: UserFormValues) => {
        try {
            const user = await Agent.Account.login(creds);
            store.commonStore.setToken(user.token);
            this.startRefreshTokenTimer(user);
            runInAction(() => this.user = user);
            history.push('/activities');
            //close the modal after the user is logged in
            store.modalStore.closeModal();
            console.log(user);
        } catch (error) {
            throw error;
        }        
    }
    
    logout = () => {
        store.commonStore.setToken(null);
        window.localStorage.removeItem('jwt');
        this.user = null;
        history.push('/');
    }
    
    getUser = async () => {
        try {
            const user = await Agent.Account.current();
            store.commonStore.setToken(user.token);
            runInAction(() => this.user = user);
            this.startRefreshTokenTimer(user);
        } catch (error) {
            console.log(error);
        }
    }
    
    register = async (creds: UserFormValues) => {
        try {
            const user = await Agent.Account.register(creds);
            store.commonStore.setToken(user.token);
            this.startRefreshTokenTimer(user);
            runInAction(() => this.user = user);
            history.push('/activities');
            //close the modal after the user is logged in
            store.modalStore.closeModal();
            console.log(user);
        } catch (error) {
            throw error;
        }
    }
    
    setImage = (image: string) => {
        if (this.user) this.user.image = image;
    }
    
    //this method with async automatically returns a promise
    getFacebookLoginStatus = async () => {
        try {
            window.FB.getLoginStatus(response => {
                if (response.status === 'connected') {
                    this.fbAccessToken = response.authResponse.accessToken;
                }
            })
        } catch (error) {
            console.log(error);
        }

    }
    
    //also logs any responses in strings from Facebok
    facebookLogin = () => {
        this.fbLoading = true;
        const apiLogin = (accessToken: string) => {
            Agent.Account.fbLogin(accessToken).then(user => {
                store.commonStore.setToken(user.token);
                this.startRefreshTokenTimer(user);
                runInAction(() => {
                    this.user = user;
                    this.fbLoading = false;
                })
                history.push('/activities');
            }).catch(error => {
                console.log(error);
                runInAction(() => {this.fbLoading = false;
                })
            })
        }
        if (this.fbAccessToken) {
            apiLogin(this.fbAccessToken);
        } else {
            window.FB.login(response => {
                apiLogin(response.authResponse.accessToken);
            }, {scope: 'public_profile,email'})
        }
    }
    
    refreshToken = async () => {
        this.stopRefreshTokenTimer();
        try {
            const user = await Agent.Account.refreshToken();
            runInAction(() => this.user = user);
            store.commonStore.setToken(user.token);
            this.startRefreshTokenTimer(user);
        } catch (error) {
            console.log(error);
        }
    }
    
    private startRefreshTokenTimer(user: User) {
        //the payload is the 2nd part of the token, so [1]
        const jwtToken = JSON.parse(atob(user.token.split('.')[1]));
        const expires = new Date(jwtToken.xp * 1000);
        //sets timer 30 seconds before it expires (probably want to do a minute or longer in production
        const timeout = expires.getTime() - Date.now() - (60 * 1000);
        //attempt to refresh the token 30 seconds before it expires
        this.refreshTokenTimeout = setTimeout(this.refreshToken, timeout);
    }
    
    private stopRefreshTokenTimer() {
        clearTimeout(this.refreshTokenTimeout);
    }
}