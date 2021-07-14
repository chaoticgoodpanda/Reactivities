import { observer } from 'mobx-react-lite';
import React from 'react';
import { Link } from 'react-router-dom';
import { Button, Container, Header, Segment, Image, Divider } from 'semantic-ui-react';
import {useStore} from "../../app/stores/Store";
import LoginForm from "../users/LoginForm";
import RegisterForm from '../users/RegisterForm';

export default observer(function HomePage() {
    const { userStore, modalStore } = useStore();
    return (
        <Segment inverted textAlign='center' vertical className='masthead'>
            <Container text>
                <Header as='h1' inverted>
                    <Image size ='massive' src='/assets/logo.png' alt='logo' style={{marginBottom: 12}} />
                    SchedulerMike
                </Header>
                {userStore.isLoggedIn ? (
                    <>
                        <Header as='h2' inverted content='Welcome to SchedulerMike!' />
                        <Button as={Link} to='/activities' size='huge' inverted>
                            Go to Activities!
                        </Button>
                    </>
                ) : (
                    <>
                        <Button onClick={() => modalStore.openModal(<LoginForm />)} to='/login' size='huge' inverted>
                        Login
                        </Button>
                        <Button onClick={() => modalStore.openModal(<RegisterForm />)} to='/login' size='huge' inverted>
                            Register
                        </Button>
                        <Divider horizontal inverted>Or </Divider>
                        <Button
                            size='huge'
                            inverted
                            color='facebook'
                            content='Login with Facebook'
                            onClick={userStore.facebookLogin}
                        />
                    </>
                        )}
            </Container>
        </Segment>

    )
})