import React from 'react';
import { Tab } from 'semantic-ui-react';
import ProfilePhotos from "./ProfilePhotos";
import {Profile} from "../../app/models/Profile";
import { observer } from 'mobx-react-lite';

interface Props {
    profile: Profile;
}

//have to add observer to anything we get from a store
export default observer(function ProfileContent({profile}: Props) {
    const panes = [
        {menuItem: 'About', render: () => <Tab.Pane>About Content</Tab.Pane>},
        {menuItem: 'Photos', render: () => <ProfilePhotos profile={profile}/>},
        {menuItem: 'Events', render: () => <Tab.Pane>Events</Tab.Pane>},
        {menuItem: 'Followers', render: () => <Tab.Pane>Followers</Tab.Pane>},
        {menuItem: 'Following', render: () => <Tab.Pane>Following</Tab.Pane>},
    ];
    
    return (
        <Tab 
            menu={{fluid: true, vertical: true}}
            menuPosition='right'
            panes={panes}
        />
    )
})