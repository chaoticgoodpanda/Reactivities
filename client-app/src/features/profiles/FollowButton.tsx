import { observer } from 'mobx-react-lite';
import React, { SyntheticEvent } from 'react';
import { Button, Reveal } from 'semantic-ui-react';
import {Profile} from "../../app/models/Profile";
import {useStore} from "../../app/stores/Store";

interface Props {
    profile: Profile;
}

export default observer(function FollowButton({profile}: Props) {
    //reason for bringing in userStore is that we don't want to show follow button if we're on the user's on profile
    const {profileStore, userStore} = useStore();
    const {updateFollowing, loading} = profileStore;

    //so we need to run a check as to whether we're on our own profile
    if (userStore.user?.username === profile.username) return null;
    
    function handleFollow(e: SyntheticEvent, username: string) {
        e.preventDefault();
        //if we're following the user already we want to unfollow so set to false; and vice versa, true
        profile.following ? updateFollowing(username, false) : updateFollowing(username, true);
    }
    
    return (
        <Reveal animated='move'>
            <Reveal.Content visible style={{width: '100%'}}>
                <Button fluid 
                        color='teal' 
                        content={profile.following ? 'Following' : 'Not Following'} />
            </Reveal.Content>
            <Reveal.Content hidden style={{width: '100%'}}>
                <Button fluid
                        basic
                        color={profile.following ? 'red' : 'green'}
                        content={profile.following ? 'Unfollow' : 'Follow'}
                        loading={loading}
                        onClick={(e) => handleFollow(e, profile.username)}
                />
            </Reveal.Content>
        </Reveal>
    )
    }
)