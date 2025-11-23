import Heading from '@/app/components/Heading';
import AuctionForm from '../AuctionForm';

export default function Page() {
  return (
    <div className="mx-auto max-w-[75%] shadow-lg p-10 bg-white rounded-lg">
      <Heading
        title="Sell Your Car"
        subtitle="Please enter the details of your car"
      ></Heading>
      <AuctionForm></AuctionForm>
    </div>
  );
}
