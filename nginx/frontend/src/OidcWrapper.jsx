import { useAuth } from "react-oidc-context";
import React from 'react';
import useAutoLogin from './UseAutoLogin'
import { Button } from "@mui/material";

function OidcWrapper({ children, spinner }) {
    const auth = useAuth();
    useAutoLogin();

    switch (auth.activeNavigator) {
        case "signinSilent":
            return spinner;
        case "signoutRedirect":
            return spinner;
    }

    if (auth.isLoading) {
        return spinner;
    }

    if (auth.error) {
        if (auth.error.message.includes('Failed to fetch')) {
            return <div class="centered-element">
            <img src='lost-connection.png' />
        
            <p>Потеряно соединение с сервером</p>
            <Button onClick={() => {
                        window.location.reload()
                    }}>Обновить</Button>
        </div>
        } else {
            auth.removeUser();
            auth.signoutRedirect({
                id_token_hint: auth.user.id_token,
                post_logout_redirect_uri: import.meta.env.VITE_CLIENT_URL,
            });
        }
    }

    if (!auth.isAuthenticated) {
        return spinner;
    }

    return children;    
}

export default OidcWrapper;