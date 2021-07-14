import React from "react";
import {Redirect, Route, RouteComponentProps, RouteProps } from "react-router-dom";
import {useStore} from "../stores/Store";

interface Props extends RouteProps {
    component: React.ComponentType<RouteComponentProps<any>> | React.ComponentType<any>;
    
}

export default function PrivateRoute({component: Component, ...rest}: Props) {
    const {userStore: {isLoggedIn}} = useStore();
    return (
        <Route 
            {...rest}
            //if they're not logged in, they get sent back to home page to protect our routes
            //note this protection is only on server side, not client side
            render={(props) => isLoggedIn ? <Component {...props} /> : <Redirect to='/' />}
        />
    )
}