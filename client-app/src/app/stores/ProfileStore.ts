import { makeAutoObservable, runInAction } from "mobx";
import {Photo, Profile } from "../models/Profile";
import Agent from "../api/agent";
import {store} from "./Store";

export default class ProfileStore {
    profile: Profile | null = null;
    loadingProfile = false;
    uploading = false;
    loading = false;
    
    constructor() {
        makeAutoObservable(this);
    }
    
    //returns true if these two usernames match
    get isCurrentUser() {
        if (store.userStore.user && this.profile) {
            return store.userStore.user.username === this.profile.username;
        }
        //returns false if no user or profile
        return false;
    }
    
    loadProfile = async (username: string) => {
        this.loadingProfile = true;
        try {
            const profile = await Agent.Profiles.get(username);
            runInAction(() => {
                this.profile = profile;
                this.loadingProfile = false;})
        } catch (error) {
            console.log(error);
            runInAction(() => this.loadingProfile = false);
        }
    }
    
    uploadPhoto = async (file: Blob) => {
        this.uploading = true;
        try {
            const response = await Agent.Profiles.uploadPhoto(file);
            const photo = response.data;
            runInAction(() => {
                if (this.profile) {
                    this.profile.photos?.push(photo);
                    if (photo.isMain && store.userStore.user) {
                        store.userStore.setImage(photo.url);
                        this.profile.image = photo.url;
                    }
                }
                this.uploading = false;
            })
        } catch (error) {
            console.log(error);
            runInAction(() => this.uploading = false)
        }
    }
    
    setMainPhoto = async (photo: Photo) => {
        this.loading = true;
        try {
            await Agent.Profiles.setMainPhoto(photo.id);
            store.userStore.setImage(photo.url);
            runInAction(() => {
                if (this.profile && this.profile.photos) {
                    //marks the existing main photo as no longer a main photo
                    this.profile.photos.find(p => p.isMain)!.isMain = false;
                    //marks the newly selected photo as the main photo
                    this.profile.photos.find(p => p.id === photo.id)!.isMain = true;
                    this.profile.image = photo.url;
                    this.loading = false;
                }
            })
        } catch (error) {
            console.log(error);
            runInAction(() => this.loading = false)
        }
    }
    
    deletePhoto = async (photo: Photo) => {
        this.loading = true;
        try {
            await Agent.Profiles.deletePhoto(photo.id);
            //not accessing the UserStore.ts here as we're not allowing user to delete their main photo
            runInAction(() => {
                if (this.profile) {
                    //returns an array of all the photos except for the one that matches the id we've presented here for deletion
                    this.profile.photos = this.profile.photos?.filter(p => p.id !== photo.id);
                    this.loading = false;
                }
            })
        } catch (error) {
            console.log(error);
            runInAction(() => this.loading = false)
        }
    }
}