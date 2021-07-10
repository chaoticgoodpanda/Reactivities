import { observer } from 'mobx-react-lite';
import React from 'react';
import { Link } from 'react-router-dom';
import { Image, List, Popup } from 'semantic-ui-react';
import { Profile } from '../../../app/models/Profile';
import ProfileCard from "../../profiles/ProfileCard";

interface Props {
    attendees: Profile[]
}


export default observer(function ActivityListItemAttendee({attendees}: Props) {
    return (
        <List horizontal>
            {attendees.map(attendee => (
                //need to give key here because Popup is first element in the attendees map
                //moved marginRight to -7 because a weird line shows up after the right margin circle icon
                <Popup 
                    hoverable
                    key={attendee.username}
                    trigger={
                        <List.Item key={attendee.username} as={Link} to={`/profiles/${attendee.username}`}>
                            <Image style={{marginRight: -7}} size='mini' circular src={attendee.image || '/assets/user.png'} />Î
                        </List.Item>
                    }
                >
                    <Popup.Content>
                        <ProfileCard profile={attendee}/>
                    </Popup.Content>
                </Popup>

            ))}

        </List>
    )
})