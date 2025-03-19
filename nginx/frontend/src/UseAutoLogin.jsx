import { hasAuthParams, useAuth } from 'react-oidc-context';
import React, { useEffect } from 'react';

const useAutoLogin = () => {
  const auth = useAuth();
  const [hasTriedSignin, setHasTriedSignin] = React.useState(false);

  useEffect(() => {
    if (!hasAuthParams() &&
        !auth.isAuthenticated && 
        !auth.activeNavigator && 
        !auth.isLoading &&
        !hasTriedSignin
    ) {
        auth.signinRedirect();
        setHasTriedSignin(true);
    }
}, [auth, hasTriedSignin]);
};

export default useAutoLogin;