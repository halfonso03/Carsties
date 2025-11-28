import EmptyFilter from '@/app/components/EmptyFilter';

export const dynamic = 'force-dynamic';

export default async function SignIn({
  searchParams,
}: {
  searchParams: Promise<{ callbackUrl: string }>;
}) {
  const { callbackUrl } = await searchParams;

  return (
    <EmptyFilter
      title="You need to be logged in to do that."
      subtitle="PLease click below to login"
      showLogin
      callbackUrl={callbackUrl}
    />
  );
}
