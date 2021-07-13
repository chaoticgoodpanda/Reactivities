import { observer } from 'mobx-react-lite';
import React, { useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { Grid } from 'semantic-ui-react';
import ProfileContent from './ProfileContent';
import ProfileHeader from "./ProfileHeader";
import {useStore} from "../../app/stores/Store";
import LoadingComponent from "../../app/layout/LoadingComponent";

export default observer(function ProfilePage() {
    const {username} = useParams<{username: string}>();
    const {profileStore} = useStore();
    const {loadingProfile, loadProfile, profile, setActiveTab} = profileStore;
    
    //useEffect() to call loadProfile method when this component loads
    useEffect(() => {
        loadProfile(username);
        return () => {
            setActiveTab(0);
        } 
    }, [loadProfile, username, setActiveTab])
    
    if (loadingProfile) return <LoadingComponent content='Loading profile...' />
    
    //need {profile && in order to get rid of what if profile=null warning
    return (
        <Grid>
            <Grid.Column width={16}>
                {profile && 
                <>
                    <ProfileHeader profile={profile}/>
                    <ProfileContent profile={profile}/>
                </>}
            </Grid.Column>
        </Grid>
    )
})