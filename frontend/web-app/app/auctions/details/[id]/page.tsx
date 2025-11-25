import { getDetailedViewData } from '@/app/actions/auctionActions';
import Heading from '@/app/components/Heading';
import CarImage from '../../CarImage';
import CountdownTimer from '../../CountdownTimer';
import DetailedSpecs from './DetailSpecs';
import EditButton from './EditButton';
import { getCurrentUser } from '@/app/actions/authActions';
import DeleteButton from './DeleteButton';
import BidList from './BidList';

export default async function Details({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  const data = await getDetailedViewData(id);
  const user = await getCurrentUser();

  return (
    <div>
      <div className="flex justify-between">
        <div className="flex items-center gap-3">
          <Heading title={`${data.make} ${data.model}`} />
          {user?.username == data.seller && (
            <>
              <EditButton id={data.id}></EditButton>
              <DeleteButton id={data.id}></DeleteButton>
            </>
          )}
        </div>

        <div className="flex gap-3">
          <h3 className="text-2xl font-semibold">Time remaining:</h3>
          <CountdownTimer auctionEnd={data.auctionEnd} />
        </div>
      </div>

      <div className="grid grid-cols-2 gap-6 mt-3">
        <div className="w-full relative aspect-16/10 bg-gray-200 rounded-lg overflow-hidden">
          <CarImage imageUrl={data.imageUrl} />
        </div>
        <BidList user={user} auction={data}></BidList>
      </div>

      <div className="mt-3 grid grid-cols-1 rounded-lg">
        <DetailedSpecs auction={data} />
      </div>
    </div>
  );
}
