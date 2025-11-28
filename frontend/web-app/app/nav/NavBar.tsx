'use client';
import { useSession } from 'next-auth/react';
import LoginButton from './LoginButton';
import Logo from './Logo';
import Search from './Search';
import UserActions from './UserActions';

export default function NavBar() {
  const session = useSession();
  const user = session.data?.user;
  return (
    <header className="sticky top-0 z-50 flex justify-between bg-white p-5 items-center text-gray-800 shadow-md">
      <Logo></Logo>
      <Search></Search>
      {user ? (
        <UserActions user={user}></UserActions>
      ) : (
        <LoginButton></LoginButton>
      )}
    </header>
  );
}
