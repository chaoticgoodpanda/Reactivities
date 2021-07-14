import { makeAutoObservable, reaction, runInAction } from "mobx";
import {Photo, Profile, UserActivity } from "../models/Profile";
import Agent from "../api/agent";
import {store} from "./Store";

export default class ProfileStore {
    profile: Profile | null = null;
    loadingProfile = false;
    uploading = false;
    loading = false;
    followings: Profile[] = [];
    loadingFollowings = false;
    activeTab = 0;
    userActivities: UserActivity[] =[];
    loadingActivities = false;
    
    constructor() {
        makeAutoObservable(this);
        
        reaction(
            () => this.activeTab,
            activeTab => {
                if (activeTab === 3 || activeTab === 4) {
                    const predicate = activeTab === 3 ? 'followers' : 'following';
                    this.loadFollowings(predicate);
                } else {
                    this.followings = [];
                }
            }
        )
    }
    
    setActiveTab = (activeTab: any) => {
        this.activeTab = activeTab;
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
    
    //status for following or unfollowing a user
    //flip the follow status, basically
    //following: boolean property is what we're about to set the property is
    //profile.following --> what the property statement currently is
    updateFollowing = async (username: string, following: boolean) => {
        this.loading = true;
        try {
            await Agent.Profiles.updateFollowing(username);
            store.activityStore.updateAttendeeFollowing(username);
            runInAction(() => {
                if (this.profile && this.profile.username !== store.userStore.user?.username && this.profile.username === username) {
                    following ? this.profile.followersCount++ : this.profile.followersCount--;
                    this.profile.following = !this.profile.following;
                }
                if (this.profile && this.profile.username === store.userStore.user?.username ) {
                    following ? this.profile.followingCount++ : this.profile.followingCount--;
                }
                this.followings.forEach(profile => {
                    if (profile.username === username) {
                        profile.following ? profile.followersCount-- : profile.followersCount++;
                        profile.following = !profile.following;
                    }
                })
                this.loading = false;
            })
        } catch (error) {
           console.log(error); 
           runInAction(() => this.loading = false);
        }
    }
    
    loadFollowings = async (predicate: string) => {
        this.loadingFollowings = true;
        try {
            const followings = await Agent.Profiles.listFollowings(this.profile!.username, predicate);
            runInAction(() => {
                this.followings = followings;
                this.loadingFollowings = false;
            })
        } catch (error) {
            console.log(error);
            runInAction(() => this.loadingFollowings = false);
        }
    }
    
    //predicate is optional because we return the default case if we don't have a predicate
    loadUserActivities = async (username: string, predicate?: string) => {
        this.loadingActivities = true;
        try {
            const activities = await Agent.Profiles.listActivities(username, predicate!);
            runInAction(() => {
                this.userActivities = activities;
                this.loadingActivities = false;
            })
        } catch (error) {
            console.log(error);
            runInAction(() => this.loadingActivities = false);
        }
    }
}