'use client';

import { useParamsStore } from '@/hooks/useParamsStore';
import { usePathname, useRouter } from 'next/navigation';
import { AiOutlineCar } from 'react-icons/ai';

export default function Logo() {
  const router = useRouter();
  const pathname = usePathname();

  function handleReset() {
    if (pathname != '/') router.push('/');
    reset();
  }
  const reset = useParamsStore((state) => state.reset);

  return (
    <div
      onClick={handleReset}
      className="cursor-pointer flex items-center gap-2 text-3xl font-semibold text-red-500"
    >
      <AiOutlineCar size={34}></AiOutlineCar>
      <div>Carsties Auctions</div>
    </div>
  );
}
